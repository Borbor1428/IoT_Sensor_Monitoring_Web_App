using IoT_Sensor_Monitoring_Web_App.Data;
using IoT_Sensor_Monitoring_Web_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoT_Sensor_Monitoring_Web_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceTypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DeviceTypesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceType>>> Get()
        {
            return await _context.DeviceTypes.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<DeviceType>> Post(DeviceType type)
        {
            _context.DeviceTypes.Add(type);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = type.DeviceTypeId }, type);
        }
    }
}
