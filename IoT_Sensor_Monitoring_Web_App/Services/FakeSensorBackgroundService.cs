using IoT_Sensor_Monitoring_Web_App.Data;
using IoT_Sensor_Monitoring_Web_App.Hubs;
using IoT_Sensor_Monitoring_Web_App.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace IoT_Sensor_Monitoring_Web_App.Services
{
    public class FakeSensorBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<SensorHub> _hubContext;
        private readonly Random _random = new();

        public FakeSensorBackgroundService(
            IServiceScopeFactory scopeFactory,
            IHubContext<SensorHub> hubContext)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Cihaz bilgisiyle birlikte tüm aktif sensörleri çek
                var sensors = await db.Sensors
                    .Include(s => s.Device)
                    .Where(s => s.IsActive && s.Device.IsActive)
                    .ToListAsync(stoppingToken);

                var now = DateTime.UtcNow;

                foreach (var sensor in sensors)
                {
                    int intervalSeconds = sensor.Device.ReadingIntervalSeconds;
                    if (intervalSeconds <= 0)
                        intervalSeconds = 5; // default

                    var cutoff = now.AddSeconds(-intervalSeconds);

                    // Bu sensörün son okumasını bul
                    var lastReading = await db.SensorReadings
                        .Where(r => r.SensorId == sensor.SensorId)
                        .OrderByDescending(r => r.RecordedAt)
                        .FirstOrDefaultAsync(stoppingToken);

                    // Eğer son okuma interval içinde ise yeni veri üretme
                    if (lastReading != null && lastReading.RecordedAt > cutoff)
                        continue;

                    double value = GenerateValueFor(sensor);

                    var reading = new SensorReading
                    {
                        SensorId = sensor.SensorId,
                        Value = value,
                        RecordedAt = now
                    };

                    db.SensorReadings.Add(reading);
                    await db.SaveChangesAsync(stoppingToken);

                    // 🔹 Alert kurallarını kontrol et
                    var rules = await db.AlertRules
                        .Where(r => r.SensorId == sensor.SensorId && r.IsActive)
                        .ToListAsync(stoppingToken);

                    foreach (var rule in rules)
                    {
                        if (IsAlertTriggered(rule, value))
                        {
                            var alert = new Alert
                            {
                                AlertRuleId = rule.AlertRuleId,
                                ReadingId = reading.ReadingId,
                                TriggeredAt = now,
                                IsAcknowledged = false
                            };

                            db.Alerts.Add(alert);
                            await db.SaveChangesAsync(stoppingToken);

                            await _hubContext.Clients.All.SendAsync(
                                "ReceiveAlert",
                                new
                                {
                                    AlertId = alert.AlertId,
                                    SensorId = sensor.SensorId,
                                    SensorName = sensor.SensorName,
                                    rule.ConditionType,
                                    rule.ThresholdValue,
                                    Value = reading.Value,
                                    alert.TriggeredAt,
                                    rule.Message
                                },
                                cancellationToken: stoppingToken
                            );
                        }
                    }

                    // 🔹 Normal reading yayını
                    await _hubContext.Clients.All.SendAsync(
                        "ReceiveReading",
                        new
                        {
                            SensorId = sensor.SensorId,
                            SensorName = sensor.SensorName,
                            MetricType = sensor.MetricType,
                            Unit = sensor.Unit,
                            Value = value,
                            RecordedAt = reading.RecordedAt
                        },
                        cancellationToken: stoppingToken
                    );
                }

                // Döngü frekansı genel; device bazlı interval içeride kontrol ediliyor
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }

        private double GenerateValueFor(Sensor sensor)
        {
            var metric = sensor.MetricType.ToLower();

            return metric switch
            {
                "temperature" => 15 + _random.NextDouble() * 15, // 15-30 °C
                "humidity" => 30 + _random.NextDouble() * 40, // 30-70 %
                "pressure" => 980 + _random.NextDouble() * 40, // 980-1020 hPa
                "airquality" => 50 + _random.NextDouble() * 100, // 50-150 (AQI/ppm basitleştirilmiş)
                _ => _random.NextDouble() * 100
            };
        }

        private bool IsAlertTriggered(AlertRule rule, double value)
        {
            return rule.ConditionType switch
            {
                ">" => value > rule.ThresholdValue,
                ">=" => value >= rule.ThresholdValue,
                "<" => value < rule.ThresholdValue,
                "<=" => value <= rule.ThresholdValue,
                "==" => Math.Abs(value - rule.ThresholdValue) < 0.0001,
                _ => false
            };
        }
    }
}