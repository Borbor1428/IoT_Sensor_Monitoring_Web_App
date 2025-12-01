namespace IoT_Sensor_Monitoring_Web_App.Models
{
    public class RetentionPolicy
    {
        public int RetentionPolicyId { get; set; }
        public string Name { get; set; } = null!;   
        public int DaysToKeep { get; set; }          
    }
}
