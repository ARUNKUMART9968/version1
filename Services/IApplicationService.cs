using BoticAPI.Models;

namespace BoticAPI.Services
{
    public interface IApplicationService
    {
        Task<(bool success, string message, int? applicationId)> CreateApplicationAsync(int applicantId, string roleName);
        Task<Application?> GetApplicationAsync(int applicationId, int userId);
        Task<IEnumerable<Application>> GetUserApplicationsAsync(int userId);
        Task<(bool success, string message)> UpdateApplicationStatusAsync(int applicationId, string newStatus, string updatedBy, string? comment);
        Task<IEnumerable<ActivityLog>> GetApplicationActivityLogsAsync(int applicationId);
    }
}