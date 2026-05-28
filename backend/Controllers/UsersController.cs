using backend.DTOs;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize(Policy = "HR_OR_ADMIN")]
    [Route("api/v1/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            var users = await _userService.GetAllAsync(includeInactive);
            return Ok(new { data = users });
        }

        [HttpPost]
        [Authorize(Policy = "IT_ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            var (success, errorCode, message, data) = await _userService.CreateAsync(dto);
            if (!success) return BadRequest(new { error = new { code = errorCode, message } });
            return CreatedAtAction(nameof(GetAll), new { }, new { data });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "IT_ADMIN")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateUserDto dto)
        {
            var (success, errorCode, message, data) = await _userService.UpdateAsync(id, dto);
            if (!success) return errorCode == "USER_NOT_FOUND"
                ? NotFound(new { error = new { code = errorCode, message } })
                : BadRequest(new { error = new { code = errorCode, message } });
            return Ok(new { message, data });
        }
    }
}
