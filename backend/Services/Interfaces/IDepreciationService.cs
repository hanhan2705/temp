using backend.DTOs;

namespace backend.Services.Interfaces
{
    public interface IDepreciationService
    {
        Task<List<object>> GetAllAsync();
        Task<(bool success, string errorCode, string message)> SetupAsync(SetupDepreciationDto dto);
        Task<object?> GetByDeviceIdAsync(long deviceId);
    }
}
