using backend.DTOs;

namespace backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool success, string errorCode, string message, AuthResponse? data)> LoginAsync(LoginRequest request);
        Task<(bool success, string message)> SeedAdminAsync();
        Task<(bool success, string message)> SeedDataAsync();
    }
}
