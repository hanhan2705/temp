using backend.Data;
using backend.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Authorize(Policy = "HR_OR_ADMIN")]
    [Route("api/v1/history")]
    public class HistoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HistoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("allocations")]
        public async Task<IActionResult> GetAllocations()
        {
            var data = await _context.AllocationHistories
                .Include(h => h.User)
                .Include(h => h.Device)
                .OrderByDescending(h => h.AllocatedAt)
                .Select(h => new
                {
                    id = h.Id,
                    userId = h.UserId,
                    code = $"CP-{h.Id}",
                    deviceName = h.Device.Name,
                    deviceCode = $"TB-{h.DeviceId:D3}",
                    employeeName = h.User.FullName,
                    date = h.AllocatedAt,
                    status = h.Status ?? "COMPLETED"
                })
                .ToListAsync();

            return Ok(new { data });
        }

        [HttpGet("recoveries")]
        public async Task<IActionResult> GetRecoveries()
        {
            var data = await _context.ReturnHistories
                .Include(h => h.User)
                .Include(h => h.Device)
                .OrderByDescending(h => h.ReturnedAt)
                .Select(h => new
                {
                    id = h.Id,
                    code = $"TH-{h.Id}",
                    deviceName = h.Device.Name,
                    deviceCode = $"TB-{h.DeviceId:D3}",
                    employeeName = h.User.FullName,
                    date = h.ReturnedAt,
                    condition = h.Condition ?? "OK"
                })
                .ToListAsync();

            return Ok(new { data });
        }

        [HttpGet("activity")]
        public async Task<IActionResult> GetActivity()
        {
            var allocations = await _context.AllocationHistories
                .Include(h => h.User)
                .Include(h => h.Device)
                .OrderByDescending(h => h.AllocatedAt)
                .Take(50)
                .Select(h => new
                {
                    type = "ALLOCATION",
                    time = h.AllocatedAt,
                    employeeName = h.User.FullName,
                    deviceName = h.Device.Name,
                    deviceCode = $"TB-{h.DeviceId:D3}",
                    status = "COMPLETED"
                })
                .ToListAsync();

            var recoveries = await _context.ReturnHistories
                .Include(h => h.User)
                .Include(h => h.Device)
                .OrderByDescending(h => h.ReturnedAt)
                .Take(50)
                .Select(h => new
                {
                    type = "RECOVERY",
                    time = h.ReturnedAt,
                    employeeName = h.User.FullName,
                    deviceName = h.Device.Name,
                    deviceCode = $"TB-{h.DeviceId:D3}",
                    status = "COMPLETED"
                })
                .ToListAsync();

            var merged = allocations
                .Concat(recoveries)
                .OrderByDescending(x => x.time)
                .Take(30)
                .ToList();

            return Ok(new { data = merged });
        }
    }
}
