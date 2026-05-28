namespace backend.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<object> GetSummaryAsync(string role, long userId);
    }
}
