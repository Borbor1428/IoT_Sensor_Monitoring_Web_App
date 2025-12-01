using IoT_Sensor_Monitoring_Web_App.Data;
using IoT_Sensor_Monitoring_Web_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoT_Sensor_Monitoring_Web_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertRulesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AlertRulesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/alertrules
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlertRule>>> GetAlertRules()
        {
            return await _context.AlertRules
                .Include(r => r.Sensor)
                .ToListAsync();
        }

        // GET: api/alertrules/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AlertRule>> GetAlertRule(int id)
        {
            var rule = await _context.AlertRules
                .Include(r => r.Sensor)
                .FirstOrDefaultAsync(r => r.AlertRuleId == id);

            if (rule == null)
                return NotFound();

            return rule;
        }

        // GET: api/alertrules/by-sensor/3
        [HttpGet("by-sensor/{sensorId}")]
        public async Task<ActionResult<IEnumerable<AlertRule>>> GetRulesForSensor(int sensorId)
        {
            var rules = await _context.AlertRules
                .Where(r => r.SensorId == sensorId)
                .ToListAsync();

            return rules;
        }

        // POST: api/alertrules
        [HttpPost]
        public async Task<ActionResult<AlertRule>> PostAlertRule(AlertRule rule)
        {
            _context.AlertRules.Add(rule);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAlertRule), new { id = rule.AlertRuleId }, rule);
        }

        // PUT: api/alertrules/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAlertRule(int id, AlertRule rule)
        {
            if (id != rule.AlertRuleId)
                return BadRequest();

            _context.Entry(rule).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.AlertRules.Any(r => r.AlertRuleId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/alertrules/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlertRule(int id)
        {
            var rule = await _context.AlertRules.FindAsync(id);
            if (rule == null)
                return NotFound();

            _context.AlertRules.Remove(rule);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}