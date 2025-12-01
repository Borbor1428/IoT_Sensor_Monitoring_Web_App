using IoT_Sensor_Monitoring_Web_App.Data;
using Microsoft.EntityFrameworkCore;

namespace IoT_Sensor_Monitoring_Web_App.Services
{
    public class RetentionCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RetentionCleanupService> _logger;

        public RetentionCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<RetentionCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RetentionCleanupService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // geçerli retention ayarını oku
                    var system = await db.SystemSettings
                        .Include(s => s.CurrentRetentionPolicy)
                        .FirstOrDefaultAsync(stoppingToken);

                    int daysToKeep = system?.CurrentRetentionPolicy?.DaysToKeep ?? 30;
                    var cutoff = DateTime.UtcNow.AddDays(-daysToKeep);

                    _logger.LogInformation(
                        "Retention cleanup running. DaysToKeep={Days}, Cutoff={Cutoff}",
                        daysToKeep, cutoff);

                    // Önce eski Alerts sil
                    var oldAlerts = db.Alerts
                        .Where(a => a.TriggeredAt < cutoff);

                    // Sonra eski SensorReadings sil
                    var oldReadings = db.SensorReadings
                        .Where(r => r.RecordedAt < cutoff);

                    db.Alerts.RemoveRange(oldAlerts);
                    db.SensorReadings.RemoveRange(oldReadings);

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RetentionCleanupService.");
                }

                // Çok sık olmasına gerek yok – 5 dakikada bir kontrol yeterli
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation("RetentionCleanupService stopped.");
        }
    }
}