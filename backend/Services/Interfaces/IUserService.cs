using backend.DTOs;

namespace backend.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<object>> GetAllAsync(bool includeInactive);
        Task<(bool success, string errorCode, string message, object? data)> CreateAsync(CreateUserDto dto);
        Task<(bool success, string errorCode, string message, object? data)> UpdateAsync(long id, UpdateUserDto dto);
    }
}
