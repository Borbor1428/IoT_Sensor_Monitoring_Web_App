using IoT_Sensor_Monitoring_Web_App.Models;
using Microsoft.EntityFrameworkCore;

namespace IoT_Sensor_Monitoring_Web_App.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<DeviceType> DeviceTypes { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<SensorReading> SensorReadings { get; set; }
        public DbSet<AlertRule> AlertRules { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<RetentionPolicy> RetentionPolicies { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SystemSetting>()
                .HasOne(s => s.CurrentRetentionPolicy)
                .WithMany()
                .HasForeignKey(s => s.CurrentRetentionPolicyId);

            modelBuilder.Entity<Sensor>()
                .HasOne(s => s.Device)
                .WithMany(d => d.Sensors)
                .HasForeignKey(s => s.DeviceId);

            modelBuilder.Entity<SensorReading>()
                .HasOne(r => r.Sensor)
                .WithMany(s => s.Readings)
                .HasForeignKey(r => r.SensorId);

            modelBuilder.Entity<AlertRule>()
                .HasOne(a => a.Sensor)
                .WithMany(s => s.AlertRules)
                .HasForeignKey(a => a.SensorId);

            modelBuilder.Entity<Alert>()
                .HasOne(a => a.AlertRule)
                .WithMany(r => r.Alerts)
                .HasForeignKey(a => a.AlertRuleId)
                .OnDelete(DeleteBehavior.Restrict);   

            modelBuilder.Entity<Alert>()
                .HasOne(a => a.Reading)
                .WithMany(r => r.Alerts)
                .HasForeignKey(a => a.ReadingId)
                .OnDelete(DeleteBehavior.Restrict);   
        }
    }
}