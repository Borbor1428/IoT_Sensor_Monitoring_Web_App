using IoT_Sensor_Monitoring_Web_App.Data;
using IoT_Sensor_Monitoring_Web_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoT_Sensor_Monitoring_Web_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SensorsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/sensors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sensor>>> GetSensors()
        {
            return await _context.Sensors
                .Include(s => s.Device)
                .ToListAsync();
        }

       
        [HttpGet("{id}")]
        public async Task<ActionResult<Sensor>> GetSensor(int id)
        {
            var sensor = await _context.Sensors
                .Include(s => s.Device)
                .FirstOrDefaultAsync(s => s.SensorId == id);

            if (sensor == null)
                return NotFound();

            return sensor;
        }

      
        [HttpPost]
        public async Task<ActionResult<Sensor>> PostSensor(Sensor sensor)
        {
            _context.Sensors.Add(sensor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSensor), new { id = sensor.SensorId }, sensor);
        }

       
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSensor(int id, Sensor sensor)
        {
            if (id != sensor.SensorId)
                return BadRequest();

            _context.Entry(sensor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Sensors.Any(e => e.SensorId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSensor(int id)
        {
            var sensor = await _context.Sensors.FindAsync(id);
            if (sensor == null)
                return NotFound();

            _context.Sensors.Remove(sensor);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("{id}/readings")]
        public async Task<ActionResult<IEnumerable<SensorReadingDto>>> GetSensorReadings(
            int id,
            [FromQuery] int hours = 24)
        {
            if (!_context.Sensors.Any(s => s.SensorId == id))
                return NotFound();

            var cutoff = DateTime.UtcNow.AddHours(-hours);

            var readings = await _context.SensorReadings
                .Where(r => r.SensorId == id && r.RecordedAt >= cutoff)
                .OrderBy(r => r.RecordedAt)
                .Select(r => new SensorReadingDto
                {
                    ReadingId = r.ReadingId,
                    Value = r.Value,
                    RecordedAt = r.RecordedAt
                })
                .ToListAsync();

            return readings;
        }

        
        [HttpGet("{id}/alerts")]
        public async Task<ActionResult<IEnumerable<SensorAlertDto>>> GetSensorAlerts(
            int id,
            [FromQuery] int hours = 24)
        {
            if (!_context.Sensors.Any(s => s.SensorId == id))
                return NotFound();

            var cutoff = DateTime.UtcNow.AddHours(-hours);

            var alerts = await _context.Alerts
                .Include(a => a.AlertRule)
                .Include(a => a.Reading)
                .Where(a => a.AlertRule.SensorId == id && a.TriggeredAt >= cutoff)
                .OrderByDescending(a => a.TriggeredAt)
                .ToListAsync();

            var result = alerts.Select(a => new SensorAlertDto
            {
                AlertId = a.AlertId,
                TriggeredAt = a.TriggeredAt,
                Value = a.Reading?.Value ?? 0,
                ConditionType = a.AlertRule?.ConditionType ?? "",
                ThresholdValue = a.AlertRule?.ThresholdValue ?? 0,
                Message = a.AlertRule?.Message ?? ""
            }).ToList();

            return result;
        }

       
        public class SensorReadingDto
        {
            public int ReadingId { get; set; }
            public double Value { get; set; }
            public DateTime RecordedAt { get; set; }
        }

        public class SensorAlertDto
        {
            public int AlertId { get; set; }
            public DateTime TriggeredAt { get; set; }
            public double Value { get; set; }
            public string ConditionType { get; set; } = "";
            public double ThresholdValue { get; set; }
            public string Message { get; set; } = "";
        }

    }

}     