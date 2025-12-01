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

                // Tüm aktif sensörler
                var sensors = await db.Sensors
                    .Where(s => s.IsActive)
                    .Include(s => s.Device)
                    .ToListAsync(stoppingToken);

                foreach (var sensor in sensors)
                {
                    double generatedValue = GenerateValueFor(sensor);

                    var reading = new SensorReading
                    {
                        SensorId = sensor.SensorId,
                        Value = generatedValue,
                        RecordedAt = DateTime.UtcNow
                    };

                    db.SensorReadings.Add(reading);
                    await db.SaveChangesAsync(stoppingToken);

                  
                    await _hubContext.Clients.All.SendAsync("ReceiveReading", new
                    {
                        SensorId = sensor.SensorId,
                        SensorName = sensor.SensorName,
                        Value = generatedValue,
                        RecordedAt = reading.RecordedAt,
                        Unit = sensor.Unit,
                        MetricType = sensor.MetricType
                    }, cancellationToken: stoppingToken);

                 
                    var rules = await db.AlertRules
                        .Where(r => r.SensorId == sensor.SensorId && r.IsActive)
                        .ToListAsync(stoppingToken);

                    foreach (var rule in rules)
                    {
                        if (IsTriggered(rule, generatedValue))
                        {
                            
                            var alert = new Alert
                            {
                                AlertRuleId = rule.AlertRuleId,
                                ReadingId = reading.ReadingId,
                                TriggeredAt = DateTime.UtcNow,
                                IsAcknowledged = false
                            };

                            db.Alerts.Add(alert);
                            await db.SaveChangesAsync(stoppingToken);

                          
                            await _hubContext.Clients.All.SendAsync("ReceiveAlert", new
                            {
                                SensorId = sensor.SensorId,
                                SensorName = sensor.SensorName,
                                Value = generatedValue,
                                TriggeredAt = alert.TriggeredAt,
                                ConditionType = rule.ConditionType,
                                ThresholdValue = rule.ThresholdValue,
                                Message = rule.Message
                            }, cancellationToken: stoppingToken);
                        }
                    }
                }

              
                await Task.Delay(5000, stoppingToken);
            }
        }

        private bool IsTriggered(AlertRule rule, double value)
        {
          
            var cond = (rule.ConditionType ?? "").Trim().ToLower();

            return cond switch
            {
                "above" => value > rule.ThresholdValue,
                "below" => value < rule.ThresholdValue,
                _ => false
            };
        }

        private double GenerateValueFor(Sensor sensor)
        {
            var metric = (sensor.MetricType ?? "").Trim().ToLower();

            return metric switch
            {
                "temperature" => 15 + _random.NextDouble() * 15, // 15 - 30 °C
                "humidity" => 30 + _random.NextDouble() * 40, // 30 - 70 %
                "pressure" => 980 + _random.NextDouble() * 40, // 980 - 1020 hPa
                "airquality" => 30 + _random.NextDouble() * 120, // 30 - 150 AQI
                _ => _random.NextDouble() * 100       // 0 - 100 default
            };
        }
    }
}