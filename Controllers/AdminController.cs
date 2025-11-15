using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BoticAPI.Services;
using BoticAPI.Data;
using BoticAPI.Models;
using BoticAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BoticAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly BoticDbContext _context;
        private readonly IApplicationService _applicationService;

        public AdminController(BoticDbContext context, IApplicationService applicationService)
        {
            _context = context;
            _applicationService = applicationService;
        }

        private string GetUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        }

        /// <summary>
        /// Create a new job role
        /// </summary>
        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Role name is required" });
            }

            if (await _context.Roles.AnyAsync(r => r.Name == request.Name))
            {
                return BadRequest(new { message = "Role already exists" });
            }

            var role = new Role
            {
                Name = request.Name,
                IsTechnical = request.IsTechnical
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return Ok(new { roleId = role.Id, message = "Role created successfully" });
        }

        /// <summary>
        /// Get all non-technical applications
        /// </summary>
        [HttpGet("applications")]
        public async Task<IActionResult> GetAllApplications()
        {
            var apps = await _context.Applications
                .Include(a => a.RoleApplied)
                .Include(a => a.Applicant)
                .Where(a => !a.RoleApplied.IsTechnical)
                .Select(a => new
                {
                    a.Id,
                    a.CurrentStatus,
                    a.CreatedAt,
                    RoleApplied = a.RoleApplied.Name,
                    Applicant = a.Applicant.Name,
                    ApplicantEmail = a.Applicant.Email
                })
                .ToListAsync();

            return Ok(apps);
        }

        /// <summary>
        /// Update non-technical application status
        /// </summary>
        [HttpPut("applications/{id}/status")]
        public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewStatus))
            {
                return BadRequest(new { message = "New status is required" });
            }

            var app = await _context.Applications
                .Include(a => a.RoleApplied)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null)
            {
                return NotFound(new { message = "Application not found" });
            }

            if (app.RoleApplied.IsTechnical)
            {
                return BadRequest(new { message = "Cannot manually update technical role applications" });
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
        /// Get all system users
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    Role = u.Role.Name
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// Get all available roles
        /// </summary>
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _context.Roles
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.IsTechnical,
                    ApplicationCount = r.Applications.Count
                })
                .ToListAsync();

            return Ok(roles);
        }

        /// <summary>
        /// Get all applications (technical and non-technical)
        /// </summary>
        [HttpGet("all-applications")]
        public async Task<IActionResult> GetAllApplicationsUnfiltered()
        {
            var apps = await _context.Applications
                .Include(a => a.RoleApplied)
                .Include(a => a.Applicant)
                .Select(a => new
                {
                    a.Id,
                    a.CurrentStatus,
                    a.CreatedAt,
                    RoleApplied = a.RoleApplied.Name,
                    IsTechnical = a.RoleApplied.IsTechnical,
                    Applicant = a.Applicant.Name,
                    ApplicantEmail = a.Applicant.Email
                })
                .ToListAsync();

            return Ok(apps);
        }
    }
}