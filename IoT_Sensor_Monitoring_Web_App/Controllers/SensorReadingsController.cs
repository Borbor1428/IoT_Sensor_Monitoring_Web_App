using IoT_Sensor_Monitoring_Web_App.Data;
using IoT_Sensor_Monitoring_Web_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoT_Sensor_Monitoring_Web_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorReadingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SensorReadingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/sensorreadings/by-sensor/5?minutes=60
        [HttpGet("by-sensor/{sensorId}")]
        public async Task<ActionResult<IEnumerable<SensorReading>>> GetReadingsForSensor(
            int sensorId,
            [FromQuery] int minutes = 60)
        {
            var fromTime = DateTime.UtcNow.AddMinutes(-minutes);

            var readings = await _context.SensorReadings
                .Where(r => r.SensorId == sensorId && r.RecordedAt >= fromTime)
                .OrderBy(r => r.RecordedAt)
                .ToListAsync();

            return readings;
        }

        // (Opsiyonel) direkt son N kaydı getir
        // GET: api/sensorreadings/last/5?sensorId=1
        [HttpGet("last/{count}")]
        public async Task<ActionResult<IEnumerable<SensorReading>>> GetLastReadings(
            int count,
            [FromQuery] int sensorId)
        {
            var readings = await _context.SensorReadings
                .Where(r => r.SensorId == sensorId)
                .OrderByDescending(r => r.RecordedAt)
                .Take(count)
                .OrderBy(r => r.RecordedAt)
                .ToListAsync();

            return readings;
        }
    }
}