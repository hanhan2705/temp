using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<object>> GetAllAsync(bool includeInactive)
        {
            var query = _context.Users.AsQueryable();
            if (!includeInactive)
                query = query.Where(u => u.Status);

            var users = await query
                .OrderBy(u => u.FullName)
                .Select(u => new { u.Id, u.FullName, u.Email, u.Role, u.Status })
                .ToListAsync();

            return users.Cast<object>().ToList();
        }

        public async Task<(bool success, string errorCode, string message, object? data)> CreateAsync(CreateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Email))
                return (false, "VALIDATION_ERROR", "Họ tên và email không được để trống", null);

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
                return (false, "VALIDATION_ERROR", "Mật khẩu tối thiểu 8 ký tự", null);

            var role = NormalizeRole(dto.Role);
            if (role == null)
                return (false, "VALIDATION_ERROR", "Vai trò không hợp lệ", null);

            var email = dto.Email.Trim().ToLowerInvariant();
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return (false, "EMAIL_EXISTS", "Email đã tồn tại", null);

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                Role = role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Status = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "", "Tạo người dùng thành công",
                new { user.Id, user.FullName, user.Email, user.Role, user.Status });
        }

        public async Task<(bool success, string errorCode, string message, object? data)> UpdateAsync(long id, UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return (false, "USER_NOT_FOUND", "Không tìm thấy người dùng", null);

            if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Email))
                return (false, "VALIDATION_ERROR", "Họ tên và email không được để trống", null);

            var role = NormalizeRole(dto.Role);
            if (role == null)
                return (false, "VALIDATION_ERROR", "Vai trò không hợp lệ", null);

            var email = dto.Email.Trim().ToLowerInvariant();
            if (await _context.Users.AnyAsync(u => u.Email == email && u.Id != id))
                return (false, "EMAIL_EXISTS", "Email đã tồn tại", null);

            user.FullName = dto.FullName.Trim();
            user.Email = email;
            user.Role = role;
            user.Status = dto.Status;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password.Length < 8)
                    return (false, "VALIDATION_ERROR", "Mật khẩu tối thiểu 8 ký tự", null);
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();
            return (true, "", "Cập nhật người dùng thành công",
                new { user.Id, user.FullName, user.Email, user.Role, user.Status });
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
