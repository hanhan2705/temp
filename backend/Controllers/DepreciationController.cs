using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Authorize(Policy = "IT_ADMIN")]
    [Route("api/v1/depreciations")]
    public class DepreciationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepreciationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
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

                // Calculate months passed
                int monthsPassed = ((now.Year - purchaseDate.Value.Year) * 12) + now.Month - purchaseDate.Value.Month;
                if (monthsPassed < 0) monthsPassed = 0;

                int life = dep.UsefulLifeMonths ?? 36;
                decimal salvage = dep.SalvageValue ?? 0;

                decimal newValue = dep.CurrentValue ?? originalCost.Value;

                if (dep.Method == "STRAIGHT_LINE")
                {
                    if (monthsPassed >= life)
                    {
                        newValue = salvage;
                    }
                    else
                    {
                        decimal totalDepreciable = originalCost.Value - salvage;
                        decimal monthlyDep = totalDepreciable / life;
                        newValue = originalCost.Value - (monthlyDep * monthsPassed);
                        if (newValue < salvage) newValue = salvage;
                    }
                }
                else if (dep.Method == "DECLINING")
                {
                    double rate = 2.0 / life;
                    decimal val = originalCost.Value;
                    for (int i = 0; i < monthsPassed; i++)
                    {
                        val = val * (decimal)(1.0 - rate);
                    }
                    newValue = val;
                    if (newValue < salvage) newValue = salvage;
                }

                newValue = Math.Round(newValue, 2);
                if (dep.CurrentValue != newValue)
                {
                    dep.CurrentValue = newValue;
                    _context.Entry(dep).Property(x => x.CurrentValue).IsModified = true;
                    changed = true;
                }
            }

            if (changed)
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new { data });
        }

        [HttpPost]
        public async Task<IActionResult> Setup([FromBody] SetupDepreciationDto dto)
        {
            var device = await _context.Devices.FindAsync(dto.DeviceId);
            if (device == null)
                return NotFound(new { error = new { code = "DEVICE_NOT_FOUND", message = "Không tìm thấy thiết bị" } });

            if (device.PurchaseDate == null || device.OriginalCost == null || device.OriginalCost <= 0)
                return BadRequest(new { error = new { code = "MISSING_INFO", message = "Thiết bị thiếu thông tin ngày mua hoặc nguyên giá để lập khấu hao" } });

            var existing = await _context.Depreciations.FirstOrDefaultAsync(d => d.DeviceId == dto.DeviceId);
            if (existing != null)
                return BadRequest(new { error = new { code = "ALREADY_EXISTS", message = "Thiết bị đã có khấu hao" } });

            var depreciation = new Depreciation
            {
                DeviceId = dto.DeviceId,
                Method = dto.Method,
                CurrentValue = dto.InitialValue,
                UsefulLifeMonths = dto.UsefulLifeMonths,
                SalvageValue = dto.SalvageValue,
                Period = dto.Period
            };

            _context.Depreciations.Add(depreciation);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thiết lập khấu hao thành công", deviceId = dto.DeviceId });
        }

        [HttpGet("{deviceId}")]
        public async Task<IActionResult> GetByDeviceId(long deviceId)
        {
            var dep = await _context.Depreciations.FirstOrDefaultAsync(d => d.DeviceId == deviceId);
            if (dep == null) return NotFound();
            return Ok(dep);
        }
    }
}
