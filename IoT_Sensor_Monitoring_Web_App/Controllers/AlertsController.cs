using IoT_Sensor_Monitoring_Web_App.Data;
using IoT_Sensor_Monitoring_Web_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoT_Sensor_Monitoring_Web_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AlertsController(AppDbContext context)
        {
            _context = context;
        }

       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Alert>>> GetAlerts()
        {
            return await _context.Alerts
                .Include(a => a.AlertRule)
                .Include(a => a.Reading)
                .OrderByDescending(a => a.TriggeredAt)
                .ToListAsync();
        }

        [HttpGet("unacknowledged")]
        public async Task<ActionResult<IEnumerable<Alert>>> GetUnacknowledged()
        {
            return await _context.Alerts
                .Where(a => !a.IsAcknowledged)
                .OrderByDescending(a => a.TriggeredAt)
                .ToListAsync();
        }

      
        [HttpPut("ack/{id}")]
        public async Task<IActionResult> Acknowledge(int id)
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert == null)
                return NotFound();

            alert.IsAcknowledged = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
