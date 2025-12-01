using IoT_Sensor_Monitoring_Web_App.Data;
using IoT_Sensor_Monitoring_Web_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoT_Sensor_Monitoring_Web_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RetentionPoliciesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RetentionPoliciesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/retentionpolicies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RetentionPolicy>>> GetPolicies()
        {
            return await _context.RetentionPolicies.ToListAsync();
        }

        // GET: api/retentionpolicies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RetentionPolicy>> GetPolicy(int id)
        {
            var policy = await _context.RetentionPolicies.FindAsync(id);
            if (policy == null)
                return NotFound();

            return policy;
        }

        // POST: api/retentionpolicies
        [HttpPost]
        public async Task<ActionResult<RetentionPolicy>> PostPolicy(RetentionPolicy policy)
        {
            _context.RetentionPolicies.Add(policy);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPolicy), new { id = policy.RetentionPolicyId }, policy);
        }

        // PUT: api/retentionpolicies/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPolicy(int id, RetentionPolicy policy)
        {
            if (id != policy.RetentionPolicyId)
                return BadRequest();

            _context.Entry(policy).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.RetentionPolicies.Any(p => p.RetentionPolicyId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/retentionpolicies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePolicy(int id)
        {
            var policy = await _context.RetentionPolicies.FindAsync(id);
            if (policy == null)
                return NotFound();

            _context.RetentionPolicies.Remove(policy);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}