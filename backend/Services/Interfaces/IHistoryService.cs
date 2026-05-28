namespace backend.Services.Interfaces
{
    public interface IHistoryService
    {
        Task<List<object>> GetAllocationsAsync();
        Task<List<object>> GetRecoveriesAsync();
        Task<List<object>> GetActivityAsync();
    }
}
