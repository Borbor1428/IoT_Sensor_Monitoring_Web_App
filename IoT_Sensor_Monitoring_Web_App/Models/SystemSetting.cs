namespace IoT_Sensor_Monitoring_Web_App.Models
{
    public class SystemSetting
    {
        public int SystemSettingId { get; set; }     
        public int? CurrentRetentionPolicyId { get; set; }

        public RetentionPolicy? CurrentRetentionPolicy { get; set; }
    }
}
