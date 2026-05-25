using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Authorize(Policy = "HR_OR_ADMIN")]
    [Route("api/v1/users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            var query = _context.Users.AsQueryable();
            if (!includeInactive)
                query = query.Where(u => u.Status);

            var users = await query
                .OrderBy(u => u.FullName)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.Status
                })
                .ToListAsync();

            return Ok(new { data = users });
        }

        [HttpPost]
        [Authorize(Policy = "IT_ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Họ tên và email không được để trống" } });

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
                return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Mật khẩu tối thiểu 8 ký tự" } });

            var role = NormalizeRole(dto.Role);
            if (role == null)
                return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Vai trò không hợp lệ" } });

            var email = dto.Email.Trim().ToLowerInvariant();
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return BadRequest(new { error = new { code = "EMAIL_EXISTS", message = "Email đã tồn tại" } });

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                Role = role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Status = true
            };

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { error = new { code = "DB_ERROR", message = "Không lưu được người dùng. Khởi động lại backend để đồng bộ ID database." } });
            }

            return CreatedAtAction(nameof(GetAll), new { id = user.Id }, new
            {
                data = new { user.Id, user.FullName, user.Email, user.Role, user.Status }
            });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "IT_ADMIN")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { error = new { code = "USER_NOT_FOUND", message = "Không tìm thấy người dùng" } });

            if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Họ tên và email không được để trống" } });

            var role = NormalizeRole(dto.Role);
            if (role == null)
                return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Vai trò không hợp lệ" } });

            var email = dto.Email.Trim().ToLowerInvariant();
            if (await _context.Users.AnyAsync(u => u.Email == email && u.Id != id))
                return BadRequest(new { error = new { code = "EMAIL_EXISTS", message = "Email đã tồn tại" } });

            user.FullName = dto.FullName.Trim();
            user.Email = email;
            user.Role = role;
            user.Status = dto.Status;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password.Length < 8)
                    return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Mật khẩu tối thiểu 8 ký tự" } });
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật người dùng thành công", data = new { user.Id, user.FullName, user.Email, user.Role, user.Status } });
        }

        private static string? NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role)) return null;
            return role.Trim().ToUpperInvariant() switch
            {
                "IT_ADMIN" or "IT ADMIN" or "ITADMIN" => "IT_ADMIN",
                "HR" => "HR",
                "EMPLOYEE" or "NHAN VIEN" or "NHÂN VIÊN" => "EMPLOYEE",
                _ => null
            };
        }
    }
}
