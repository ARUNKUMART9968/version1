namespace BoticAPI.Services
{
    public interface IDashboardService
    {
        Task<dynamic> GetDashboardMetricsAsync(int userId, string userRole);
    }
}