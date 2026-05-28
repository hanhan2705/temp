using backend.DTOs;
using backend.Helpers;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize(Policy = "ALL_ROLES")]
    [Route("api/v1/requests")]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _requestService;

        public RequestController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? type, [FromQuery] string? status)
        {
            var role = User.GetUserRole();
            var userId = User.GetUserId();
            var requests = await _requestService.GetAllAsync(type, status, role, userId);
            return Ok(new { data = requests });
        }

        [HttpPost("allocation")]
        [Authorize(Policy = "HR_OR_ADMIN")]
        public async Task<IActionResult> CreateAllocation([FromBody] CreateAllocationRequestDto dto)
        {
            var (success, errorCode, message, data) = await _requestService.CreateAllocationAsync(dto, User.GetUserId());
            if (!success) return BadRequest(new { error = new { code = errorCode, message } });
            return Ok(new { data, message });
        }

        [HttpPost("allocation/self")]
        [Authorize(Roles = "EMPLOYEE")]
        public async Task<IActionResult> CreateAllocationSelf([FromBody] CreateAllocationRequestDto dto)
        {
            var (success, errorCode, message, data) = await _requestService.CreateAllocationSelfAsync(dto, User.GetUserId());
            if (!success) return BadRequest(new { error = new { code = errorCode, message } });
            return Ok(new { data, message });
        }

        [HttpPost("recovery")]
        [Authorize(Policy = "HR_OR_ADMIN")]
        public async Task<IActionResult> CreateRecovery([FromBody] CreateRecoveryRequestDto dto)
        {
            var (success, errorCode, message, data) = await _requestService.CreateRecoveryAsync(dto, User.GetUserId());
            if (!success) return BadRequest(new { error = new { code = errorCode, message } });
            return Ok(new { data, message });
        }

        [HttpPatch("{id}/approve-allocation")]
        [Authorize(Policy = "IT_ADMIN")]
        public async Task<IActionResult> ApproveAllocation(long id, [FromBody] ApproveAllocationRequestDto dto)
        {
            var (success, errorCode, message) = await _requestService.ApproveAllocationAsync(id, dto);
            if (!success) return errorCode == "NOT_FOUND"
                ? NotFound(new { error = new { code = errorCode, message } })
                : BadRequest(new { error = new { code = errorCode, message } });
            return Ok(new { message, requestStatus = "APPROVED", deviceStatus = "ASSIGNED" });
        }

        [HttpPatch("{id}/confirm-recovery")]
        [Authorize(Policy = "IT_ADMIN")]
        public async Task<IActionResult> ConfirmRecovery(long id, [FromBody] ApproveRecoveryRequestDto dto)
        {
            var (success, errorCode, message) = await _requestService.ConfirmRecoveryAsync(id, dto);
            if (!success) return errorCode == "NOT_FOUND"
                ? NotFound(new { error = new { code = errorCode, message } })
                : BadRequest(new { error = new { code = errorCode, message } });
            return Ok(new { message, requestStatus = "COMPLETED", deviceStatus = "AVAILABLE" });
        }

        [HttpPatch("{id}/reject")]
        [Authorize(Policy = "IT_ADMIN")]
        public async Task<IActionResult> RejectRequest(long id, [FromBody] RejectRequestDto dto)
        {
            var (success, errorCode, message) = await _requestService.RejectAsync(id);
            if (!success) return errorCode == "NOT_FOUND"
                ? NotFound(new { error = new { code = errorCode, message } })
                : BadRequest(new { error = new { code = errorCode, message } });
            return Ok(new { message, requestStatus = "REJECTED" });
        }
    }
}
