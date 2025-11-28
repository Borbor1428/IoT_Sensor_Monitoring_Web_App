namespace IoT_Sensor_Monitoring_Web_App.Models
{
    public class Device
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; } = null!; // Örn: "Device-001"
        public int DeviceTypeId { get; set; }
        public string? Location { get; set; }           // Örn: "Lab 1"
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DeviceType? DeviceType { get; set; }
        public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
    }
}
