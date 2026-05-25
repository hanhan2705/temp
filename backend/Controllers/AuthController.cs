using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // For this project, username is email.
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { error = new { code = "INVALID_CREDENTIALS", message = "Sai email hoặc mật khẩu" } });
            }

            if (!user.Status)
            {
                 return Unauthorized(new { error = new { code = "ACCOUNT_DISABLED", message = "Tài khoản đã bị vô hiệu hóa" } });
            }

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponse
            {
                AccessToken = token,
                User = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role
                }
            });
        }

        [HttpPost("seed")]
        public async Task<IActionResult> SeedAdmin()
        {
            if (await _context.Users.AnyAsync())
            {
                return BadRequest("Đã có dữ liệu user.");
            }

            var admin = new User
            {
                FullName = "Admin",
                Email = "admin@company.com",
                Role = "IT_ADMIN",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                Status = true
            };

            var hr = new User
            {
                FullName = "HR Lan",
                Email = "hr@company.com",
                Role = "HR",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                Status = true
            };

            var emp = new User
            {
                FullName = "Nhân viên Mai",
                Email = "mai@company.com",
                Role = "EMPLOYEE",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                Status = true
            };

            _context.Users.AddRange(admin, hr, emp);
            await _context.SaveChangesAsync();

            return Ok("Khởi tạo dữ liệu người dùng mẫu thành công.");
        }

        [HttpPost("seed-data")]
        [Authorize(Roles = "IT_ADMIN")]
        public async Task<IActionResult> SeedData()
        {
            if (await _context.Devices.AnyAsync())
            {
                return BadRequest("Đã có dữ liệu thiết bị.");
            }

            var hr = await _context.Users.FirstOrDefaultAsync(u => u.Role == "HR");
            var emp = await _context.Users.FirstOrDefaultAsync(u => u.Role == "EMPLOYEE");

            var device1 = new Device { Name = "MacBook Pro M2", Type = "Laptop", OriginalCost = 35000000, Status = "ASSIGNED", PurchaseDate = DateTime.UtcNow.AddMonths(-6), AssignedUserId = emp?.Id };
            var device2 = new Device { Name = "Dell UltraSharp 27", Type = "Monitor", OriginalCost = 12000000, Status = "AVAILABLE", PurchaseDate = DateTime.UtcNow.AddMonths(-2) };
            var device3 = new Device { Name = "ThinkPad T14", Type = "Laptop", OriginalCost = 28000000, Status = "AVAILABLE", PurchaseDate = DateTime.UtcNow.AddMonths(-1) };
            
            _context.Devices.AddRange(device1, device2, device3);
            await _context.SaveChangesAsync();

            var req1 = new Request { UserId = emp?.Id ?? 1, CreatedByUserId = emp?.Id, RequestType = "ALLOCATION", Status = "PENDING", CreatedAt = DateTime.UtcNow.AddDays(-2), Reason = "Cần laptop mới" };
            var req2 = new Request { UserId = emp?.Id ?? 1, CreatedByUserId = hr?.Id, RequestType = "RECOVERY", Status = "APPROVED", CreatedAt = DateTime.UtcNow.AddDays(-5), Reason = "Nghỉ việc" };
            _context.Requests.AddRange(req1, req2);
            await _context.SaveChangesAsync();

            _context.RequestDevices.Add(new RequestDevice { RequestId = req1.Id, DeviceId = device3.Id, Quantity = 1, Note = "Cần máy mới" });
            _context.RequestDevices.Add(new RequestDevice { RequestId = req2.Id, DeviceId = device1.Id, Quantity = 1, Note = "Thu hồi máy cũ" });

            _context.Depreciations.Add(new Depreciation { DeviceId = device1.Id, CurrentValue = 30000000, Method = "STRAIGHT_LINE" });
            _context.Depreciations.Add(new Depreciation { DeviceId = device2.Id, CurrentValue = 11000000, Method = "STRAIGHT_LINE" });
            await _context.SaveChangesAsync();

            return Ok("Khởi tạo dữ liệu thiết bị, yêu cầu, khấu hao thành công.");
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "SuperSecretKeyForDeviceManagementSystem123456!";
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FullName", user.FullName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2), // Token expires in 2 hours
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
