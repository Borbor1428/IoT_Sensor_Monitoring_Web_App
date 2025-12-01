using System.ComponentModel.DataAnnotations;

namespace IoT_Sensor_Monitoring_Web_App.Models
{
    public class SensorReading
    {
        [Key]                            
        public int ReadingId { get; set; }

        public int SensorId { get; set; }

        public double Value { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        public Sensor? Sensor { get; set; }

        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}
