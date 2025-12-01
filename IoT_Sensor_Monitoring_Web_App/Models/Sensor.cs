namespace IoT_Sensor_Monitoring_Web_App.Models
{
    public class Sensor
    {
        public int SensorId { get; set; }
        public int DeviceId { get; set; }
        public string SensorName { get; set; } = null!; 
        public string MetricType { get; set; } = null!; 
        public string Unit { get; set; } = null!;       

        public bool IsActive { get; set; } = true;

        public Device? Device { get; set; }
        public ICollection<SensorReading> Readings { get; set; } = new List<SensorReading>();
        public ICollection<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
    }
}
