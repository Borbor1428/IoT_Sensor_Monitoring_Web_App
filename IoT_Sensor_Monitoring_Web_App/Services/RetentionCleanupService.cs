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
            // Her 10 dakikada bir retention kontrolü yapalım
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var settings = await db.SystemSettings
                        .Include(s => s.CurrentRetentionPolicy)
                        .FirstOrDefaultAsync(stoppingToken);

                    if (settings?.CurrentRetentionPolicy != null)
                    {
                        var days = settings.CurrentRetentionPolicy.DaysToKeep;
                        var cutoff = DateTime.UtcNow.AddDays(-days);

                        _logger.LogInformation("RetentionCleanup: deleting data older than {Cutoff}", cutoff);

                        // Önce eski reading'lere bağlı alarmları sil
                        var oldAlerts = await db.Alerts
                            .Include(a => a.Reading)
                            .Where(a => a.Reading.RecordedAt < cutoff)
                            .ToListAsync(stoppingToken);

                        if (oldAlerts.Any())
                        {
                            db.Alerts.RemoveRange(oldAlerts);
                            await db.SaveChangesAsync(stoppingToken);
                        }

                        // Sonra eski sensor reading'leri sil
                        var oldReadings = await db.SensorReadings
                            .Where(r => r.RecordedAt < cutoff)
                            .ToListAsync(stoppingToken);

                        if (oldReadings.Any())
                        {
                            db.SensorReadings.RemoveRange(oldReadings);
                            await db.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while running retention cleanup.");
                }

                // 10 dakika bekle
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }
}