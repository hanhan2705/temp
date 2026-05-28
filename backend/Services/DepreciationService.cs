using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class DepreciationService : IDepreciationService
    {
        private readonly AppDbContext _context;

        public DepreciationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<object>> GetAllAsync()
        {
            var data = await _context.Depreciations.Include(d => d.Device).ToListAsync();
            var now = DateTime.UtcNow;
            bool changed = false;

            foreach (var dep in data)
            {
                if (dep.Device == null) continue;

                var purchaseDate = dep.Device.PurchaseDate;
                var originalCost = dep.Device.OriginalCost;
                if (purchaseDate == null || originalCost == null || originalCost <= 0) continue;

                int monthsPassed = Math.Max(0,
                    ((now.Year - purchaseDate.Value.Year) * 12) + now.Month - purchaseDate.Value.Month);

                int life = dep.UsefulLifeMonths ?? 36;
                decimal salvage = dep.SalvageValue ?? 0;
                decimal newValue = CalculateCurrentValue(dep.Method, originalCost.Value, salvage, life, monthsPassed);

                if (dep.CurrentValue != newValue)
                {
                    dep.CurrentValue = newValue;
                    _context.Entry(dep).Property(x => x.CurrentValue).IsModified = true;
                    changed = true;
                }
            }

            if (changed)
                await _context.SaveChangesAsync();

            return data.Cast<object>().ToList();
        }

        public async Task<(bool success, string errorCode, string message)> SetupAsync(SetupDepreciationDto dto)
        {
            var device = await _context.Devices.FindAsync(dto.DeviceId);
            if (device == null)
                return (false, "DEVICE_NOT_FOUND", "Không tìm thấy thiết bị");

            if (device.PurchaseDate == null || device.OriginalCost == null || device.OriginalCost <= 0)
                return (false, "MISSING_INFO", "Thiết bị thiếu thông tin ngày mua hoặc nguyên giá");

            var existing = await _context.Depreciations.FirstOrDefaultAsync(d => d.DeviceId == dto.DeviceId);
            if (existing != null)
                return (false, "ALREADY_EXISTS", "Thiết bị đã có khấu hao");

            _context.Depreciations.Add(new Depreciation
            {
                DeviceId = dto.DeviceId,
                Method = dto.Method,
                CurrentValue = dto.InitialValue,
                UsefulLifeMonths = dto.UsefulLifeMonths,
                SalvageValue = dto.SalvageValue,
                Period = dto.Period
            });

            await _context.SaveChangesAsync();
            return (true, "", "Thiết lập khấu hao thành công");
        }

        public async Task<object?> GetByDeviceIdAsync(long deviceId)
        {
            return await _context.Depreciations.FirstOrDefaultAsync(d => d.DeviceId == deviceId);
        }

        // Logic tính khấu hao tách riêng — dễ test, dễ mở rộng
        private static decimal CalculateCurrentValue(
            string? method, decimal originalCost, decimal salvage, int lifeMonths, int monthsPassed)
        {
            if (method == "DECLINING")
            {
                double rate = 2.0 / lifeMonths;
                decimal val = originalCost;
                for (int i = 0; i < monthsPassed; i++)
                    val *= (decimal)(1.0 - rate);
                return Math.Round(Math.Max(val, salvage), 2);
            }

            // Mặc định: STRAIGHT_LINE
            if (monthsPassed >= lifeMonths)
                return salvage;

            decimal monthly = (originalCost - salvage) / lifeMonths;
            decimal result = originalCost - (monthly * monthsPassed);
            return Math.Round(Math.Max(result, salvage), 2);
        }
    }
}
