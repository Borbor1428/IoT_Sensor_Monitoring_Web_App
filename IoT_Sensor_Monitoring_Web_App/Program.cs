using IoT_Sensor_Monitoring_Web_App.Data; // <- kendi namespace'ine göre düzelt
using IoT_Sensor_Monitoring_Web_App.Hubs;
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

var app = builder.Build();

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
