namespace IoT_Sensor_Monitoring_Web_App.Models
{
    public class AlertRule
    {
        public int AlertRuleId { get; set; }
        public int SensorId { get; set; }

        // ">", "<", ">=", "<=", "=="
        public string ConditionType { get; set; } = ">";
        public double ThresholdValue { get; set; }

        public string Message { get; set; } = "Threshold exceeded!";
        public bool IsActive { get; set; } = true;

        public Sensor? Sensor { get; set; }
        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}
