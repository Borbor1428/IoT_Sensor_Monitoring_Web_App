using IoT_Sensor_Monitoring_Web_App.Data; // <- kendi namespace'ine göre düzelt
using IoT_Sensor_Monitoring_Web_App.Hubs;
using IoT_Sensor_Monitoring_Web_App.Models;
using IoT_Sensor_Monitoring_Web_App.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();



// ✅ DbContext'i ekliyoruz (SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSignalR();

builder.Services.AddHostedService<FakeSensorBackgroundService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<RetentionCleanupService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Eğer hiç Device yoksa, örnek kayıtlar ekleyelim
    if (!db.Devices.Any())
    {
        // 1) DeviceType
        var tempNodeType = new DeviceType
        {
            TypeName = "Temperature & Humidity Node"
        };
        db.DeviceTypes.Add(tempNodeType);
        db.SaveChanges();

        // 2) Device
        var device = new Device
        {
            DeviceName = "Demo Device 1",
            DeviceTypeId = tempNodeType.DeviceTypeId,
            Location = "Lab 1",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Devices.Add(device);
        db.SaveChanges();

        // 3) Sensors
        var tempSensor = new Sensor
        {
            DeviceId = device.DeviceId,
            SensorName = "Temperature Sensor",
            MetricType = "Temperature",
            Unit = "°C",
            IsActive = true
        };

        var humiditySensor = new Sensor
        {
            DeviceId = device.DeviceId,
            SensorName = "Humidity Sensor",
            MetricType = "Humidity",
            Unit = "%",
            IsActive = true
        };

        db.Sensors.Add(tempSensor);
        db.Sensors.Add(humiditySensor);
        db.SaveChanges();
    }
}
app.UseHttpsRedirection();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<SensorHub>("/hubs/sensor");

app.Run();
