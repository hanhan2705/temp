using backend.Data;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetSummaryAsync(string role, long userId)
        {
            if (role == "EMPLOYEE")
                return await GetEmployeeSummaryAsync(userId);

            return await GetOrgSummaryAsync();
        }

        private async Task<object> GetOrgSummaryAsync()
        {
            var totalDevices    = await _context.Devices.CountAsync(d => d.Status != "DISPOSED");
            var assignedDevices = await _context.Devices.CountAsync(d => d.Status == "ASSIGNED");
            var availableDevices = await _context.Devices.CountAsync(d => d.Status == "AVAILABLE");
            var pendingRequests = await _context.Requests.CountAsync(r => r.Status == "PENDING");

            var recentRequests = await _context.Requests
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new
                {
                    id = r.Id,
                    employeeName = r.User.FullName,
                    requestType = r.RequestType,
                    status = r.Status,
                    date = r.CreatedAt
                })
                .ToListAsync();

            return new
            {
                scope = "ORG",
                totalDevices,
                assignedDevices,
                availableDevices,
                pendingRequests,
                expiringDepreciationDevices = 0,
                recentRequests
            };
        }

        private async Task<object> GetEmployeeSummaryAsync(long userId)
        {
            var myDevices    = await _context.Devices.CountAsync(d => d.AssignedUserId == userId && d.Status == "ASSIGNED");
            var myPending    = await _context.Requests.CountAsync(r => r.UserId == userId && r.Status == "PENDING");
            var myRejected   = await _context.Requests.CountAsync(r => r.UserId == userId && r.Status == "REJECTED");
            var myCompleted  = await _context.Requests.CountAsync(r => r.UserId == userId && r.Status == "APPROVED");

            var recentNotifications = await _context.Requests
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new
                {
                    id = r.Id,
                    requestType = r.RequestType,
                    status = r.Status,
                    date = r.CreatedAt,
                    message = r.Reason
                })
                .ToListAsync();

            return new
            {
                scope = "PERSONAL",
                myDevices,
                myPendingRequests = myPending,
                myRejectedRequests = myRejected,
                myCompletedRequests = myCompleted,
                recentNotifications
            };
        }
    }
}
