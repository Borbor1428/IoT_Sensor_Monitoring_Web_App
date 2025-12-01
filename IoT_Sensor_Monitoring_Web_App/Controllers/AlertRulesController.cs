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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlertRule>>> GetAlertRules()
        {
            return await _context.AlertRules
                .Include(r => r.Sensor)
                .ToListAsync();
        }

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

     
        [HttpPost]
        public async Task<ActionResult<AlertRule>> PostAlertRule(AlertRule rule)
        {
            
            if (!await _context.Sensors.AnyAsync(s => s.SensorId == rule.SensorId))
            {
                return BadRequest($"SensorId {rule.SensorId} için bir sensör bulunamadı.");
            }

            _context.AlertRules.Add(rule);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAlertRule), new { id = rule.AlertRuleId }, rule);
        }

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
                if (!await _context.AlertRules.AnyAsync(r => r.AlertRuleId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

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