using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using backend.Helpers;

namespace backend.Controllers
{
    [ApiController]
    [Authorize(Policy = "ALL_ROLES")]
    [Route("api/v1/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DevicesController(AppDbContext context)
        {
            _context = context;
        }

        private static DateTime? ToUtc(DateTime? dt)
        {
            if (!dt.HasValue) return null;
            var v = dt.Value;
            return v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Date, DateTimeKind.Utc);
        }

        private IQueryable<Device> ApplyRoleFilter(IQueryable<Device> query)
        {
            var role = User.GetUserRole();
            var userId = User.GetUserId();

            if (role == "EMPLOYEE")
                return query.Where(d => d.AssignedUserId == userId && d.Status == "ASSIGNED");

            return query;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? status)
        {
            try
            {
                var query = _context.Devices
                    .Include(d => d.AssignedUser)
                    .AsQueryable();

                query = ApplyRoleFilter(query);

                if (!string.IsNullOrEmpty(keyword))
                {
                    var cleanKeyword = keyword.Replace("TB-", "", StringComparison.OrdinalIgnoreCase)
                                              .Replace("TB", "", StringComparison.OrdinalIgnoreCase)
                                              .TrimStart('0')
                                              .Trim();
                    if (long.TryParse(cleanKeyword, out long searchId))
                    {
                        query = query.Where(d => d.Name.Contains(keyword) || d.Id == searchId);
                    }
                    else
                    {
                        query = query.Where(d => d.Name.Contains(keyword));
                    }
                }

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(d => d.Status == status);

                var devices = await query.OrderByDescending(d => d.Id).ToListAsync();
                return Ok(new { data = devices });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = new { code = "DB_ERROR", message = ex.Message } });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var query = ApplyRoleFilter(_context.Devices.Include(d => d.AssignedUser));
            var device = await query.FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
                return NotFound(new { error = new { code = "DEVICE_NOT_FOUND", message = "Không tìm thấy thiết bị" } });

            return Ok(device);
        }

        [HttpPost]
        [Authorize(Policy = "IT_ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreateDeviceDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Tên thiết bị không được để trống" } });

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

            return CreatedAtAction(nameof(GetById), new { id = device.Id }, device);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "IT_ADMIN")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateDeviceDto dto)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound(new { error = new { code = "DEVICE_NOT_FOUND", message = "Không tìm thấy thiết bị" } });

            if (device.Status == "DISPOSED")
                return BadRequest(new { error = new { code = "DEVICE_DISPOSED", message = "Thiết bị đã thanh lý, không thể sửa" } });

            device.Name = dto.Name.Trim();
            device.Type = dto.Type;
            device.PurchaseDate = ToUtc(dto.PurchaseDate);
            device.OriginalCost = dto.OriginalCost;
            device.Status = dto.Status;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thiết bị thành công", data = device });
        }

        [HttpPatch("{id}/dispose")]
        [Authorize(Policy = "IT_ADMIN")]
        public async Task<IActionResult> Dispose(long id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound(new { error = new { code = "DEVICE_NOT_FOUND", message = "Không tìm thấy thiết bị" } });

            if (device.Status == "DISPOSED")
                return BadRequest(new { error = new { code = "ALREADY_DISPOSED", message = "Thiết bị đã được thanh lý" } });

            device.Status = "DISPOSED";
            device.AssignedUserId = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Chuyển thiết bị sang trạng thái Thanh lý thành công", status = "DISPOSED" });
        }
    }
}
