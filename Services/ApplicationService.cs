using BoticAPI.Data;
using BoticAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BoticAPI.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly BoticDbContext _context;

        public ApplicationService(BoticDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message, int? applicationId)> CreateApplicationAsync(int applicantId, string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
            {
                return (false, "Role not found", null);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == applicantId);
            if (user == null)
            {
                return (false, "Applicant not found", null);
            }

            var application = new Application
            {
                ApplicantId = applicantId,
                RoleAppliedId = role.Id,
                CurrentStatus = "Applied"
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            var activityLog = new ActivityLog
            {
                ApplicationId = application.Id,
                OldStatus = null,
                NewStatus = "Applied",
                UpdatedBy = user.Email,
                UpdatedByRole = "Applicant",
                Comment = "Application created"
            };

            _context.ActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();

            return (true, "Application created successfully", application.Id);
        }

        public async Task<Application?> GetApplicationAsync(int applicationId, int userId)
        {
            var app = await _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.RoleApplied)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (app == null || app.ApplicantId != userId)
            {
                return null;
            }

            return app;
        }

        public async Task<IEnumerable<Application>> GetUserApplicationsAsync(int userId)
        {
            return await _context.Applications
                .Where(a => a.ApplicantId == userId)
                .Include(a => a.RoleApplied)
                .Include(a => a.Applicant)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<(bool success, string message)> UpdateApplicationStatusAsync(int applicationId, string newStatus, string updatedBy, string? comment)
        {
            var app = await _context.Applications.FirstOrDefaultAsync(a => a.Id == applicationId);
            if (app == null)
            {
                return (false, "Application not found");
            }

            var oldStatus = app.CurrentStatus;
            app.CurrentStatus = newStatus;

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == updatedBy);

            var activityLog = new ActivityLog
            {
                ApplicationId = applicationId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                UpdatedBy = updatedBy,
                UpdatedByRole = user?.Role.Name ?? "System",
                Comment = comment
            };

            _context.ActivityLogs.Add(activityLog);
            _context.Applications.Update(app);
            await _context.SaveChangesAsync();

            return (true, "Application status updated successfully");
        }

        public async Task<IEnumerable<ActivityLog>> GetApplicationActivityLogsAsync(int applicationId)
        {
            return await _context.ActivityLogs
                .Where(a => a.ApplicationId == applicationId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}
