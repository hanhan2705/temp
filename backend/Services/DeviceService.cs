using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly AppDbContext _context;

        public DeviceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<object>> GetAllAsync(string? keyword, string? status, string role, long userId)
        {
            var query = _context.Devices
                .Include(d => d.AssignedUser)
                .AsQueryable();

            // EMPLOYEE chỉ thấy thiết bị được gán cho mình
            if (role == "EMPLOYEE")
                query = query.Where(d => d.AssignedUserId == userId && d.Status == "ASSIGNED");

            if (!string.IsNullOrEmpty(keyword))
            {
                var clean = keyword
                    .Replace("TB-", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("TB", "", StringComparison.OrdinalIgnoreCase)
                    .TrimStart('0')
                    .Trim();

                if (long.TryParse(clean, out long searchId))
                    query = query.Where(d => d.Name.Contains(keyword) || d.Id == searchId);
                else
                    query = query.Where(d => d.Name.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(status))
                query = query.Where(d => d.Status == status);

            var devices = await query.OrderByDescending(d => d.Id).ToListAsync();
            return devices.Cast<object>().ToList();
        }

        public async Task<object?> GetByIdAsync(long id, string role, long userId)
        {
            var query = _context.Devices.Include(d => d.AssignedUser).AsQueryable();

            if (role == "EMPLOYEE")
                query = query.Where(d => d.AssignedUserId == userId && d.Status == "ASSIGNED");

            return await query.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<object> CreateAsync(CreateDeviceDto dto)
        {
            var device = new Device
            {
                Name = dto.Name.Trim(),
                Type = dto.Type,
                PurchaseDate = ToUtc(dto.PurchaseDate),
                OriginalCost = dto.OriginalCost,
                Status = string.IsNullOrEmpty(dto.Status) ? "AVAILABLE" : dto.Status,
                AssignedUserId = null
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task<(bool success, string message, object? data)> UpdateAsync(long id, UpdateDeviceDto dto)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return (false, "DEVICE_NOT_FOUND", null);

            if (device.Status == "DISPOSED")
                return (false, "DEVICE_DISPOSED", null);

            device.Name = dto.Name.Trim();
            device.Type = dto.Type;
            device.PurchaseDate = ToUtc(dto.PurchaseDate);
            device.OriginalCost = dto.OriginalCost;
            device.Status = dto.Status;

            await _context.SaveChangesAsync();
            return (true, "Cập nhật thiết bị thành công", device);
        }

        public async Task<(bool success, string message)> DisposeAsync(long id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return (false, "DEVICE_NOT_FOUND");

            if (device.Status == "DISPOSED")
                return (false, "ALREADY_DISPOSED");

            device.Status = "DISPOSED";
            device.AssignedUserId = null;
            await _context.SaveChangesAsync();

            return (true, "Chuyển thiết bị sang trạng thái Thanh lý thành công");
        }

        private static DateTime? ToUtc(DateTime? dt)
        {
            if (!dt.HasValue) return null;
            var v = dt.Value;
            return v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Date, DateTimeKind.Utc);
        }
    }
}
