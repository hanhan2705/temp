using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class RequestService : IRequestService
    {
        private readonly AppDbContext _context;

        public RequestService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<object>> GetAllAsync(string? type, string? status, string role, long userId)
        {
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
            return requests.Cast<object>().ToList();
        }

        public async Task<(bool success, string errorCode, string message, object? data)> CreateAllocationAsync(
            CreateAllocationRequestDto dto, long currentUserId)
        {
            if (dto.TargetEmployeeId <= 0)
                return (false, "VALIDATION_ERROR", "Thiếu nhân viên nhận cấp phát", null);

            var request = new Request
            {
                UserId = dto.TargetEmployeeId,
                CreatedByUserId = currentUserId,
                RequestType = "ALLOCATION",
                Status = "PENDING",
                Reason = dto.Reason?.Trim()
            };

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            return (true, "", "Gửi yêu cầu cấp phát thành công",
                new { id = request.Id, requestType = "ALLOCATION", status = "PENDING" });
        }

        public async Task<(bool success, string errorCode, string message, object? data)> CreateAllocationSelfAsync(
            CreateAllocationRequestDto dto, long currentUserId)
        {
            var request = new Request
            {
                UserId = currentUserId,
                CreatedByUserId = currentUserId,
                RequestType = "ALLOCATION",
                Status = "PENDING",
                Reason = dto.Reason?.Trim()
            };

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            return (true, "", "Gửi yêu cầu cấp phát thành công",
                new { id = request.Id, requestType = "ALLOCATION", status = "PENDING" });
        }

        public async Task<(bool success, string errorCode, string message, object? data)> CreateRecoveryAsync(
            CreateRecoveryRequestDto dto, long currentUserId)
        {
            var device = await _context.Devices.FindAsync(dto.DeviceId);
            if (device == null || device.Status != "ASSIGNED")
                return (false, "DEVICE_NOT_ASSIGNED", "Thiết bị không ở trạng thái đã cấp phát", null);

            if (device.AssignedUserId != dto.TargetEmployeeId)
                return (false, "DEVICE_OWNER_MISMATCH", "Thiết bị không thuộc nhân viên này", null);

            var request = new Request
            {
                UserId = dto.TargetEmployeeId,
                CreatedByUserId = currentUserId,
                RequestType = "RECOVERY",
                Status = "PENDING",
                Reason = dto.Reason?.Trim()
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

            return (true, "", "Gửi yêu cầu thu hồi thành công",
                new { id = request.Id, requestType = "RECOVERY", status = "PENDING" });
        }

        public async Task<(bool success, string errorCode, string message)> ApproveAllocationAsync(
            long id, ApproveAllocationRequestDto dto)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null)
                return (false, "NOT_FOUND", "Không tìm thấy yêu cầu");

            if (request.RequestType != "ALLOCATION")
                return (false, "INVALID_REQUEST_TYPE", "Yêu cầu không phải là yêu cầu cấp phát");

            if (request.Status != "PENDING")
                return (false, "INVALID_STATUS", "Yêu cầu đã được xử lý trước đó");

            var device = await _context.Devices.FindAsync(dto.DeviceId);
            if (device == null || device.Status != "AVAILABLE")
                return (false, "DEVICE_NOT_AVAILABLE", "Thiết bị không khả dụng");

            request.Status = "APPROVED";
            device.Status = "ASSIGNED";
            device.AssignedUserId = request.UserId;

            _context.RequestDevices.Add(new RequestDevice
            {
                RequestId = id,
                DeviceId = dto.DeviceId,
                Quantity = 1,
                Note = dto.Note
            });

            _context.AllocationHistories.Add(new AllocationHistory
            {
                UserId = request.UserId,
                DeviceId = dto.DeviceId,
                Status = "COMPLETED"
            });

            await _context.SaveChangesAsync();
            return (true, "", "Duyệt và cấp phát thành công");
        }

        public async Task<(bool success, string errorCode, string message)> ConfirmRecoveryAsync(
            long id, ApproveRecoveryRequestDto dto)
        {
            var request = await _context.Requests
                .Include(r => r.RequestDevices)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return (false, "NOT_FOUND", "Không tìm thấy yêu cầu");

            if (request.RequestType != "RECOVERY")
                return (false, "INVALID_REQUEST_TYPE", "Yêu cầu không phải là yêu cầu thu hồi");

            if (request.Status != "PENDING")
                return (false, "INVALID_STATUS", "Yêu cầu đã được xử lý trước đó");

            var requestDevice = request.RequestDevices.FirstOrDefault();
            if (requestDevice == null)
                return (false, "NO_DEVICE", "Yêu cầu không có thiết bị");

            var device = await _context.Devices.FindAsync(requestDevice.DeviceId);
            if (device == null)
                return (false, "DEVICE_NOT_FOUND", "Không tìm thấy thiết bị");

            request.Status = "COMPLETED";
            device.Status = "AVAILABLE";
            device.AssignedUserId = null;

            _context.ReturnHistories.Add(new ReturnHistory
            {
                UserId = request.UserId,
                DeviceId = device.Id,
                Condition = dto.DeviceCondition
            });

            await _context.SaveChangesAsync();
            return (true, "", "Xác nhận thu hồi thành công");
        }

        public async Task<(bool success, string errorCode, string message)> RejectAsync(long id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null)
                return (false, "NOT_FOUND", "Không tìm thấy yêu cầu");

            if (request.Status != "PENDING")
                return (false, "INVALID_STATUS", "Yêu cầu đã được xử lý trước đó");

            request.Status = "REJECTED";
            await _context.SaveChangesAsync();
            return (true, "", "Từ chối yêu cầu thành công");
        }
    }
}
