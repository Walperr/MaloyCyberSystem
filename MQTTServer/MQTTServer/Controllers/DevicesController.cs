using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MQTTServer.DataContexts;
using MQTTServer.Dto;
using MQTTServer.Misc;

namespace MQTTServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DevicesController : ControllerBase
{
    private readonly ApplicationDataContext _context;

    public DevicesController(ApplicationDataContext context)
    {
        _context = context;
    }

    // GET: api/Devices
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Device>>> GetDevices()
    {
        if (_context.Devices == null!)
            return NotFound();

        return await _context.Devices.Select(d => new Device
        {
            DeviceID = d.DeviceID,
            DeviceName = d.DeviceName
        }).ToListAsync();
    }

    // GET: api/Devices/Connected/5
    [HttpGet("Connected/{id}")]
    public async Task<ActionResult<IEnumerable<Device>>> GetConnectedDevices(string id)
    {
        if (_context.Users == null!)
            return NotFound();

        var user = await _context.Users.FindAsync(id);

        if (user == null) 
            return NotFound();

        var connectedDevices = user.ConnectedDevicesID;

        if (connectedDevices is null)
            return new(Enumerable.Empty<Device>());

        return await _context.Devices.Where(d => connectedDevices.Contains(d.DeviceID)).Select(d => new Device
        {
            DeviceID = d.DeviceID,
            DeviceName = d.DeviceName
        }).ToListAsync();
    }

    [HttpGet("Data/Interval/{deviceID}")]
    public async Task<ActionResult<DeviceValues>> GetValues(string deviceID, [FromQuery] TimeBounds timeBounds)
    {
        var device = await _context.Devices.FindAsync(deviceID);

        if (device is null)
            return DeviceValues.Empty;

        var times = device.Times;

        if (times is null || device.Values is null)
            return DeviceValues.Empty;

        int startIndex = times.QuickSearch(t => t >= timeBounds.BeginTime);
        int endIndex = times.QuickSearch(t => t > timeBounds.EndTime);

        if (startIndex < 0) startIndex = 0;
        if (endIndex > times.Length) endIndex = times.Length;

        if (startIndex >= endIndex)
            return DeviceValues.Empty;

        return new DeviceValues { Times = times[startIndex..endIndex], Values = device.Values[startIndex..endIndex] };
    }

    // PUT: api/Devices/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDevice(string id, Models.Device device)
    {
        if (id != device.DeviceID) return BadRequest();

        _context.Entry(device).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DeviceExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    private bool DeviceExists(string id)
    {
        return (_context.Devices?.Any(e => e.DeviceID == id)).GetValueOrDefault();
    }
}