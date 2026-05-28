using backend.DTOs;
using backend.Helpers;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize(Policy = "ALL_ROLES")]
    [Route("api/v1/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;

        public DevicesController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? status)
        {
            var role = User.GetUserRole();
            var userId = User.GetUserId();
            var devices = await _deviceService.GetAllAsync(keyword, status, role, userId);
            return Ok(new { data = devices });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var role = User.GetUserRole();
            var userId = User.GetUserId();
            var device = await _deviceService.GetByIdAsync(id, role, userId);

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

            var device = await _deviceService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = ((dynamic)device).Id }, device);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "IT_ADMIN")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateDeviceDto dto)
        {
            var (success, message, data) = await _deviceService.UpdateAsync(id, dto);

            return message switch
            {
                "DEVICE_NOT_FOUND" => NotFound(new { error = new { code = message, message = "Không tìm thấy thiết bị" } }),
                "DEVICE_DISPOSED"  => BadRequest(new { error = new { code = message, message = "Thiết bị đã thanh lý, không thể sửa" } }),
                _ => Ok(new { message = "Cập nhật thiết bị thành công", data })
            };
        }

        [HttpPatch("{id}/dispose")]
        [Authorize(Policy = "IT_ADMIN")]
        public async Task<IActionResult> Dispose(long id)
        {
            var (success, message) = await _deviceService.DisposeAsync(id);

            return message switch
            {
                "DEVICE_NOT_FOUND"  => NotFound(new { error = new { code = message, message = "Không tìm thấy thiết bị" } }),
                "ALREADY_DISPOSED"  => BadRequest(new { error = new { code = message, message = "Thiết bị đã được thanh lý" } }),
                _ => Ok(new { message, status = "DISPOSED" })
            };
        }
    }
}
