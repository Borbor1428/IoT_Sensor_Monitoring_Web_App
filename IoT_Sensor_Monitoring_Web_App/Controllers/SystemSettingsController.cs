using IoT_Sensor_Monitoring_Web_App.Data;
using IoT_Sensor_Monitoring_Web_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoT_Sensor_Monitoring_Web_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemSettingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SystemSettingsController(AppDbContext context)
        {
            _context = context;
        }

      
        [HttpGet("current")]
        public async Task<ActionResult<SystemSetting>> GetCurrent()
        {
            var setting = await _context.SystemSettings
                .Include(s => s.CurrentRetentionPolicy)
                .FirstOrDefaultAsync();

            if (setting == null)
            {
                // yoksa oluştur
                setting = new SystemSetting();
                _context.SystemSettings.Add(setting);
                await _context.SaveChangesAsync();
            }

            return setting;
        }

        public class UpdateRetentionRequest
        {
            public int? CurrentRetentionPolicyId { get; set; }
        }

      
        [HttpPut("current")]
        public async Task<IActionResult> SetCurrentRetention(UpdateRetentionRequest request)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = new SystemSetting();
                _context.SystemSettings.Add(setting);
            }

            setting.CurrentRetentionPolicyId = request.CurrentRetentionPolicyId;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
