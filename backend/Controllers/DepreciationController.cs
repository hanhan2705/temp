using backend.DTOs;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize(Policy = "IT_ADMIN")]
    [Route("api/v1/depreciations")]
    public class DepreciationController : ControllerBase
    {
        private readonly IDepreciationService _depreciationService;

        public DepreciationController(IDepreciationService depreciationService)
        {
            _depreciationService = depreciationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _depreciationService.GetAllAsync();
            return Ok(new { data });
        }

        [HttpPost]
        public async Task<IActionResult> Setup([FromBody] SetupDepreciationDto dto)
        {
            var (success, errorCode, message) = await _depreciationService.SetupAsync(dto);
            if (!success) return errorCode == "DEVICE_NOT_FOUND"
                ? NotFound(new { error = new { code = errorCode, message } })
                : BadRequest(new { error = new { code = errorCode, message } });
            return Ok(new { message, deviceId = dto.DeviceId });
        }

        [HttpGet("{deviceId}")]
        public async Task<IActionResult> GetByDeviceId(long deviceId)
        {
            var dep = await _depreciationService.GetByDeviceIdAsync(deviceId);
            if (dep == null) return NotFound();
            return Ok(dep);
        }
    }
}
