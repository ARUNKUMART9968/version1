using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BoticAPI.Services;
using BoticAPI.DTOs;

namespace BoticAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private string GetUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        }

        /// <summary>
        /// Create a new application for a role
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RoleName))
            {
                return BadRequest(new { message = "Role name is required" });
            }

            var userId = GetUserId();
            var (success, message, applicationId) = await _applicationService.CreateApplicationAsync(userId, request.RoleName);

            if (!success)
            {
                return BadRequest(new { message });
            }

            return CreatedAtAction(nameof(GetApplication), new { id = applicationId },
                new { applicationId, message = "Application created successfully" });
        }

        /// <summary>
        /// Get application details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetApplication(int id)
        {
            var userId = GetUserId();
            var app = await _applicationService.GetApplicationAsync(id, userId);

            if (app == null)
            {
                return NotFound(new { message = "Application not found or access denied" });
            }

            return Ok(new
            {
                app.Id,
                app.CurrentStatus,
                app.CreatedAt,
                RoleApplied = app.RoleApplied.Name,
                Applicant = app.Applicant.Name
            });
        }

        /// <summary>
        /// Get all applications for the current user
        /// </summary>
        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = GetUserId();
            var apps = await _applicationService.GetUserApplicationsAsync(userId);

            return Ok(apps.Select(a => new
            {
                a.Id,
                a.CurrentStatus,
                a.CreatedAt,
                RoleApplied = a.RoleApplied.Name
            }));
        }

        /// <summary>
        /// Update application status (Admin/Bot only)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Bot")]
        public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewStatus))
            {
                return BadRequest(new { message = "New status is required" });
            }

            var userEmail = GetUserEmail();
            var (success, message) = await _applicationService.UpdateApplicationStatusAsync(id, request.NewStatus, userEmail, request.Comment);

            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message = "Application status updated successfully" });
        }

        /// <summary>
        /// Get activity logs for an application
        /// </summary>
        [HttpGet("{id}/activity-logs")]
        public async Task<IActionResult> GetActivityLogs(int id)
        {
            var logs = await _applicationService.GetApplicationActivityLogsAsync(id);

            return Ok(logs.Select(l => new
            {
                l.Id,
                l.OldStatus,
                l.NewStatus,
                l.UpdatedBy,
                l.UpdatedByRole,
                l.Comment,
                l.CreatedAt
            }));
        }
    }
}