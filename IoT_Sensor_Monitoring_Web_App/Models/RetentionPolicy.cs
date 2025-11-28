namespace IoT_Sensor_Monitoring_Web_App.Models
{
    public class RetentionPolicy
    {
        public int RetentionPolicyId { get; set; }
        public string Name { get; set; } = null!;    // "1 Week", "1 Month"
        public int DaysToKeep { get; set; }          // 7, 30 vs.
    }
}
