using BoticAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace BoticAPI.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly BoticDbContext _context;

        public DashboardService(BoticDbContext context)
        {
            _context = context;
        }

        public async Task<dynamic> GetDashboardMetricsAsync(int userId, string userRole)
        {
            if (userRole == "Applicant")
            {
                return await GetApplicantMetricsAsync(userId);
            }
            else if (userRole == "Admin")
            {
                return await GetAdminMetricsAsync();
            }
            else if (userRole == "Bot")
            {
                return await GetBotMetricsAsync();
            }

            return new { error = "Unknown role" };
        }

        private async Task<dynamic> GetApplicantMetricsAsync(int userId)
        {
            var applications = await _context.Applications
                .Where(a => a.ApplicantId == userId)
                .ToListAsync();

            var totalApps = applications.Count;
            var appliedCount = applications.Count(a => a.CurrentStatus == "Applied");
            var reviewedCount = applications.Count(a => a.CurrentStatus == "Reviewed");
            var offeredCount = applications.Count(a => a.CurrentStatus == "Offer");
            var hiredCount = applications.Count(a => a.CurrentStatus == "Hired");

            return new
            {
                TotalApplications = totalApps,
                Applied = appliedCount,
                Reviewed = reviewedCount,
                Offered = offeredCount,
                Hired = hiredCount,
                SuccessRate = totalApps > 0 ? Math.Round((double)hiredCount / totalApps * 100, 2) : 0
            };
        }

        private async Task<dynamic> GetAdminMetricsAsync()
        {
            var technicalApps = await _context.Applications.CountAsync(a => a.RoleApplied.IsTechnical);
            var nonTechnicalApps = await _context.Applications.CountAsync(a => !a.RoleApplied.IsTechnical);
            var totalUsers = await _context.Users.CountAsync();
            var botRunsCount = await _context.BotJobs.CountAsync();
            var totalRoles = await _context.Roles.CountAsync();

            return new
            {
                TotalApplications = technicalApps + nonTechnicalApps,
                TechnicalApplications = technicalApps,
                NonTechnicalApplications = nonTechnicalApps,
                TotalUsers = totalUsers,
                TotalRoles = totalRoles,
                BotRuns = botRunsCount
            };
        }

        private async Task<dynamic> GetBotMetricsAsync()
        {
            var processedByBot = await _context.ActivityLogs
                .CountAsync(al => al.UpdatedByRole == "Bot");

            var recentBotJobs = await _context.BotJobs
                .OrderByDescending(bj => bj.TriggeredAt)
                .Take(5)
                .Select(bj => new
                {
                    bj.Id,
                    bj.Status,
                    bj.TotalProcessed,
                    bj.TotalSucceeded,
                    bj.TotalFailed,
                    bj.TriggeredAt
                })
                .ToListAsync();

            var successfulTransitions = await _context.ActivityLogs
                .Where(al => al.UpdatedByRole == "Bot")
                .GroupBy(al => al.NewStatus)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return new
            {
                TotalProcessedByBot = processedByBot,
                RecentBotJobs = recentBotJobs,
                StatusTransitions = successfulTransitions
            };
        }
    }
}