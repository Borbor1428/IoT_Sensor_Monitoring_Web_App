using IoT_Sensor_Monitoring_Web_App.Data;
using IoT_Sensor_Monitoring_Web_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoT_Sensor_Monitoring_Web_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RetentionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RetentionController(AppDbContext context)
        {
            _context = context;
        }

  
        [HttpGet]
        public async Task<ActionResult<object>> GetCurrentRetention()
        {
            var system = await _context.SystemSettings
                .Include(s => s.CurrentRetentionPolicy)
                .FirstOrDefaultAsync();

            if (system?.CurrentRetentionPolicy == null)
            {
    
                return new
                {
                    hasSetting = false,
                    daysToKeep = 30
                };
            }

            return new
            {
                hasSetting = true,
                policyId = system.CurrentRetentionPolicyId,
                name = system.CurrentRetentionPolicy.Name,
                daysToKeep = system.CurrentRetentionPolicy.DaysToKeep
            };
        }

  
        [HttpPost]
        public async Task<ActionResult> SetRetention([FromBody] RetentionRequest request)
        {
            if (request.DaysToKeep <= 0)
                return BadRequest("DaysToKeep must be > 0");

            // tek bir policy kullanacağız: "Default"
            var policy = await _context.RetentionPolicies
                .FirstOrDefaultAsync(r => r.Name == "Default");

            if (policy == null)
            {
                policy = new RetentionPolicy
                {
                    Name = "Default",
                    DaysToKeep = request.DaysToKeep
                };
                _context.RetentionPolicies.Add(policy);
                await _context.SaveChangesAsync();
            }
            else
            {
                policy.DaysToKeep = request.DaysToKeep;
                await _context.SaveChangesAsync();
            }

            var system = await _context.SystemSettings.FirstOrDefaultAsync();
            if (system == null)
            {
                system = new SystemSetting
                {
                    CurrentRetentionPolicyId = policy.RetentionPolicyId
                };
                _context.SystemSettings.Add(system);
            }
            else
            {
                system.CurrentRetentionPolicyId = policy.RetentionPolicyId;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Retention updated",
                policyId = policy.RetentionPolicyId,
                daysToKeep = policy.DaysToKeep
            });
        }

        public class RetentionRequest
        {
            public int DaysToKeep { get; set; }
        }
    }
}