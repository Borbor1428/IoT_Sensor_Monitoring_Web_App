namespace IoT_Sensor_Monitoring_Web_App.Models
{
        public class DeviceType
        {
            public int DeviceTypeId { get; set; }
            public string TypeName { get; set; } = null!; // Örn: "Temperature Node"

            public ICollection<Device> Devices { get; set; } = new List<Device>();
        }

}
