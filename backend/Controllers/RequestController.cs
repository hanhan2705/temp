using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Helpers;

namespace backend.Controllers
{
    [ApiController]
    [Authorize(Policy = "ALL_ROLES")]
    [Route("api/v1/requests")]
    public class RequestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RequestController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? type, [FromQuery] string? status)
        {
            var role = User.GetUserRole();
            var userId = User.GetUserId();

            var query = _context.Requests
                .Include(r => r.User)
                .Include(r => r.CreatedByUser)
                .Include(r => r.RequestDevices)
                .ThenInclude(rd => rd.Device)
                .AsQueryable();

            if (role == "EMPLOYEE")
                query = query.Where(r => r.UserId == userId);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(r => r.RequestType == type);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            var requests = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return Ok(new { data = requests });
        }

        [HttpPost("allocation")]
        [Authorize(Policy = "HR_OR_ADMIN")]
        public async Task<IActionResult> CreateAllocation([FromBody] CreateAllocationRequestDto dto)
        {
            var currentUserId = User.GetUserId();
            var targetId = dto.TargetEmployeeId;

            if (targetId <= 0)
                return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Thiếu nhân viên nhận cấp phát" } });

            var request = new Request
            {
                UserId = targetId,
                CreatedByUserId = currentUserId,
                RequestType = "ALLOCATION",
                Status = "PENDING",
                Reason = dto.Reason?.Trim()
            };

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { id = request.Id, requestType = "ALLOCATION", status = "PENDING", message = "Gửi yêu cầu cấp phát thành công" });
        }

        [HttpPost("allocation/self")]
        [Authorize(Roles = "EMPLOYEE")]
        public async Task<IActionResult> CreateAllocationSelf([FromBody] CreateAllocationRequestDto dto)
        {
            var currentUserId = User.GetUserId();
            var request = new Request
            {
                UserId = currentUserId,
                CreatedByUserId = currentUserId,
                RequestType = "ALLOCATION",
                Status = "PENDING",
                Reason = dto.Reason
            };

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { id = request.Id, requestType = "ALLOCATION", status = "PENDING", message = "Gửi yêu cầu cấp phát thành công" });
        }

        [HttpPost("recovery")]
        [Authorize(Policy = "HR_OR_ADMIN")]
        public async Task<IActionResult> CreateRecovery([FromBody] CreateRecoveryRequestDto dto)
        {
            var device = await _context.Devices.FindAsync(dto.DeviceId);
            if (device == null || device.Status != "ASSIGNED")
                return BadRequest(new { error = new { code = "DEVICE_NOT_ASSIGNED", message = "Thiết bị không ở trạng thái đã cấp phát" } });

            if (device.AssignedUserId != dto.TargetEmployeeId)
                return BadRequest(new { error = new { code = "DEVICE_OWNER_MISMATCH", message = "Thiết bị không thuộc nhân viên này" } });

            var request = new Request
            {
                UserId = dto.TargetEmployeeId,
                CreatedByUserId = User.GetUserId(),
                RequestType = "RECOVERY",
                Status = "PENDING",
                Reason = dto.Reason
            };

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            _context.RequestDevices.Add(new RequestDevice
            {
                RequestId = request.Id,
                DeviceId = dto.DeviceId,
                Quantity = 1,
                Note = dto.Reason
            });
            await _context.SaveChangesAsync();

            return Ok(new { id = request.Id, requestType = "RECOVERY", status = "PENDING", message = "Gửi yêu cầu thu hồi thành công" });
        }

        [HttpPatch("{id}/approve-allocation")]
        [Authorize(Policy = "IT_ADMIN")]
        public async Task<IActionResult> ApproveAllocation(long id, [FromBody] ApproveAllocationRequestDto dto)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null) return NotFound(new { error = new { code = "NOT_FOUND", message = "Không tìm thấy yêu cầu" } });

            if (request.RequestType != "ALLOCATION")
                return BadRequest(new { error = new { code = "INVALID_REQUEST_TYPE", message = "Yêu cầu không phải là yêu cầu cấp phát" } });

            if (request.Status != "PENDING")
                return BadRequest(new { error = new { code = "INVALID_STATUS", message = "Yêu cầu đã được xử lý trước đó" } });

            var device = await _context.Devices.FindAsync(dto.DeviceId);
            if (device == null || device.Status != "AVAILABLE")
                return BadRequest(new { error = new { code = "DEVICE_NOT_AVAILABLE", message = "Thiết bị không khả dụng" } });

            request.Status = "APPROVED";
            device.Status = "ASSIGNED";
            device.AssignedUserId = request.UserId;

            _context.RequestDevices.Add(new RequestDevice { RequestId = id, DeviceId = dto.DeviceId, Quantity = 1, Note = dto.Note });

            _context.AllocationHistories.Add(new AllocationHistory
            {
                UserId = request.UserId,
                DeviceId = dto.DeviceId,
                Status = "COMPLETED"
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Duyệt và cấp phát thành công", requestStatus = "APPROVED", deviceStatus = "ASSIGNED" });
        }

        [HttpPatch("{id}/confirm-recovery")]
        [Authorize(Policy = "IT_ADMIN")]
        public async Task<IActionResult> ConfirmRecovery(long id, [FromBody] ApproveRecoveryRequestDto dto)
        {
            var request = await _context.Requests.Include(r => r.RequestDevices).FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return NotFound(new { error = new { code = "NOT_FOUND", message = "Không tìm thấy yêu cầu" } });

            if (request.RequestType != "RECOVERY")
                return BadRequest(new { error = new { code = "INVALID_REQUEST_TYPE", message = "Yêu cầu không phải là yêu cầu thu hồi" } });

            if (request.Status != "PENDING")
                return BadRequest(new { error = new { code = "INVALID_STATUS", message = "Yêu cầu đã được xử lý trước đó" } });

            var requestDevice = request.RequestDevices.FirstOrDefault();
            if (requestDevice == null) return BadRequest(new { error = new { code = "NO_DEVICE", message = "Yêu cầu không có thiết bị" } });

            var device = await _context.Devices.FindAsync(requestDevice.DeviceId);
            if (device == null) return NotFound(new { error = new { code = "DEVICE_NOT_FOUND", message = "Không tìm thấy thiết bị" } });

            request.Status = "COMPLETED"; // Cập nhật thành Đã hoàn tất theo Use Case
            device.Status = "AVAILABLE";
            device.AssignedUserId = null;

            _context.ReturnHistories.Add(new ReturnHistory
            {
                UserId = request.UserId,
                DeviceId = device.Id,
                Condition = dto.DeviceCondition
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Xác nhận thu hồi thành công", requestStatus = "APPROVED", deviceStatus = "AVAILABLE" });
        }

        [HttpPatch("{id}/reject")]
        [Authorize(Policy = "IT_ADMIN")]
        public async Task<IActionResult> RejectRequest(long id, [FromBody] RejectRequestDto dto)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null) return NotFound(new { error = new { code = "NOT_FOUND", message = "Không tìm thấy yêu cầu" } });

            if (request.Status != "PENDING")
                return BadRequest(new { error = new { code = "INVALID_STATUS", message = "Yêu cầu đã được xử lý trước đó" } });

            request.Status = "REJECTED";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Từ chối yêu cầu thành công", requestStatus = "REJECTED" });
        }
    }
}
