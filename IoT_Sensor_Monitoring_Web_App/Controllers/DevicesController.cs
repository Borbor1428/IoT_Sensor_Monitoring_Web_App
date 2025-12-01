using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoT_Sensor_Monitoring_Web_App.Data;
using IoT_Sensor_Monitoring_Web_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoT_Sensor_Monitoring_Web_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DevicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/devices
        // Tüm cihazları DTO olarak döner (cycle problemi yok)
        [HttpGet]
        public async Task<ActionResult> GetDevices()
        {
            var devices = await _context.Devices
                .Include(d => d.DeviceType)
                .Select(d => new
                {
                    d.DeviceId,
                    d.DeviceName,
                    d.Location,
                    d.IsActive,
                    d.ReadingIntervalSeconds,
                    DeviceType = d.DeviceType == null
                        ? null
                        : new
                        {
                            d.DeviceType.DeviceTypeId,
                            d.DeviceType.TypeName
                        }
                })
                .ToListAsync();

            return Ok(devices);
        }

        // GET: api/devices/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetDevice(int id)
        {
            var device = await _context.Devices
                .Include(d => d.DeviceType)
                .Where(d => d.DeviceId == id)
                .Select(d => new
                {
                    d.DeviceId,
                    d.DeviceName,
                    d.Location,
                    d.IsActive,
                    d.ReadingIntervalSeconds,
                    DeviceType = d.DeviceType == null
                        ? null
                        : new
                        {
                            d.DeviceType.DeviceTypeId,
                            d.DeviceType.TypeName
                        }
                })
                .FirstOrDefaultAsync();

            if (device == null)
                return NotFound();

            return Ok(device);
        }

        // POST: api/devices
        // Body'den gelen Device nesnesini aynen kaydediyoruz
        [HttpPost]
        public async Task<ActionResult<Device>> PostDevice(Device device)
        {
            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            // Type bilgisini de dolduralım (isteğe bağlı)
            await _context.Entry(device).Reference(d => d.DeviceType).LoadAsync();

            return CreatedAtAction(nameof(GetDevice), new { id = device.DeviceId }, new
            {
                device.DeviceId,
                device.DeviceName,
                device.Location,
                device.IsActive,
                device.ReadingIntervalSeconds,
                DeviceType = device.DeviceType == null
                    ? null
                    : new
                    {
                        device.DeviceType.DeviceTypeId,
                        device.DeviceType.TypeName
                    }
            });
        }

        // PUT: api/devices/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDevice(int id, Device device)
        {
            if (id != device.DeviceId)
                return BadRequest();

            _context.Entry(device).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Devices.Any(d => d.DeviceId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/devices/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound();

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
