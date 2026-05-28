using backend.DTOs;

namespace backend.Services.Interfaces
{
    public interface IDeviceService
    {
        Task<List<object>> GetAllAsync(string? keyword, string? status, string role, long userId);
        Task<object?> GetByIdAsync(long id, string role, long userId);
        Task<object> CreateAsync(CreateDeviceDto dto);
        Task<(bool success, string message, object? data)> UpdateAsync(long id, UpdateDeviceDto dto);
        Task<(bool success, string message)> DisposeAsync(long id);
    }
}
