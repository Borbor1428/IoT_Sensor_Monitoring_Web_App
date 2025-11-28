namespace IoT_Sensor_Monitoring_Web_App.Models
{
    public class Alert
    {
        public int AlertId { get; set; }
        public int AlertRuleId { get; set; }
        public int ReadingId { get; set; }

        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
        public bool IsAcknowledged { get; set; } = false;

        public AlertRule? AlertRule { get; set; }
        public SensorReading? Reading { get; set; }
    }
}
