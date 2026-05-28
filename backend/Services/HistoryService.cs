using backend.Data;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly AppDbContext _context;

        public HistoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<object>> GetAllocationsAsync()
        {
            var data = await _context.AllocationHistories
                .Include(h => h.User)
                .Include(h => h.Device)
                .OrderByDescending(h => h.AllocatedAt)
                .Select(h => (object)new
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

            return data;
        }

        public async Task<List<object>> GetRecoveriesAsync()
        {
            var data = await _context.ReturnHistories
                .Include(h => h.User)
                .Include(h => h.Device)
                .OrderByDescending(h => h.ReturnedAt)
                .Select(h => (object)new
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

            return data;
        }

        public async Task<List<object>> GetActivityAsync()
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

            return allocations
                .Concat<object>(recoveries)
                .OrderByDescending(x => ((dynamic)x).time)
                .Take(30)
                .ToList();
        }
    }
}
