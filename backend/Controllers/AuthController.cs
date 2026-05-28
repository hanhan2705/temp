using backend.DTOs;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (success, errorCode, message, data) = await _authService.LoginAsync(request);
            if (!success) return Unauthorized(new { error = new { code = errorCode, message } });
            return Ok(data);
        }

        [HttpPost("seed")]
        public async Task<IActionResult> SeedAdmin()
        {
            var (success, message) = await _authService.SeedAdminAsync();
            if (!success) return BadRequest(message);
            return Ok(message);
        }

        [HttpPost("seed-data")]
        [Authorize(Roles = "IT_ADMIN")]
        public async Task<IActionResult> SeedData()
        {
            var (success, message) = await _authService.SeedDataAsync();
            if (!success) return BadRequest(message);
            return Ok(message);
        }
    }
}
