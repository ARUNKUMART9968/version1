using BoticAPI.Data;
using BoticAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BoticAPI.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly BoticDbContext _context;

        // ✅ FIXED: Define valid status values
        private static readonly string[] ValidStatuses =
        {
            "Applied",
            "Reviewed",
            "CodingRound",
            "TechnicalInterview",
            "HRInterview",
            "Offer",
            "Hired",
            "Rejected"
        };

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
            // ✅ FIXED: Validate status is in allowed list
            if (!ValidStatuses.Contains(newStatus))
            {
                return (false, $"Invalid status. Allowed values: {string.Join(", ", ValidStatuses)}");
            }

            var app = await _context.Applications.FirstOrDefaultAsync(a => a.Id == applicationId);
            if (app == null)
            {
                return (false, "Application not found");
            }

            // ✅ FIXED: Prevent backward transitions (optional but recommended)
            var currentIndex = Array.IndexOf(ValidStatuses, app.CurrentStatus);
            var newIndex = Array.IndexOf(ValidStatuses, newStatus);

            if (newIndex < currentIndex && !(newStatus == "Rejected" && app.CurrentStatus != "Hired"))
            {
                return (false, $"Invalid transition from {app.CurrentStatus} to {newStatus}. Status can only move forward or be rejected.");
            }

            // ✅ FIXED: Use pessimistic locking to prevent concurrent updates
            try
            {
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
            catch (DbUpdateConcurrencyException)
            {
                return (false, "Application was updated by another user. Please refresh and try again.");
            }
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
