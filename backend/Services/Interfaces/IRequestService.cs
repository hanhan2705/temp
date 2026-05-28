using backend.DTOs;

namespace backend.Services.Interfaces
{
    public interface IRequestService
    {
        Task<List<object>> GetAllAsync(string? type, string? status, string role, long userId);
        Task<(bool success, string errorCode, string message, object? data)> CreateAllocationAsync(CreateAllocationRequestDto dto, long currentUserId);
        Task<(bool success, string errorCode, string message, object? data)> CreateAllocationSelfAsync(CreateAllocationRequestDto dto, long currentUserId);
        Task<(bool success, string errorCode, string message, object? data)> CreateRecoveryAsync(CreateRecoveryRequestDto dto, long currentUserId);
        Task<(bool success, string errorCode, string message)> ApproveAllocationAsync(long id, ApproveAllocationRequestDto dto);
        Task<(bool success, string errorCode, string message)> ConfirmRecoveryAsync(long id, ApproveRecoveryRequestDto dto);
        Task<(bool success, string errorCode, string message)> RejectAsync(long id);
    }
}
