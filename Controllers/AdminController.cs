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
            // ✅ FIXED: Add input validation and sanitization
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Role name is required" });
            }

            var roleName = request.Name.Trim();

            if (roleName.Length < 2 || roleName.Length > 200)
            {
                return BadRequest(new { message = "Role name must be between 2 and 200 characters" });
            }

            if (await _context.Roles.AnyAsync(r => r.Name == roleName))
            {
                return BadRequest(new { message = "Role already exists" });
            }

            var role = new Role
            {
                Name = roleName,
                IsTechnical = request.IsTechnical
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return Ok(new { roleId = role.Id, message = "Role created successfully" });
        }

        /// <summary>
        /// Get all non-technical applications with pagination
        /// </summary>
        [HttpGet("applications")]
        public async Task<IActionResult> GetAllApplications([FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            // ✅ FIXED: Add pagination parameters
            if (skip < 0 || take < 1 || take > 500)
            {
                return BadRequest(new { message = "Invalid pagination parameters. Skip must be >= 0, Take must be between 1-500" });
            }

            var totalCount = await _context.Applications
                .CountAsync(a => !a.RoleApplied.IsTechnical);

            var apps = await _context.Applications
                .Include(a => a.RoleApplied)
                .Include(a => a.Applicant)
                .Where(a => !a.RoleApplied.IsTechnical)
                .OrderByDescending(a => a.CreatedAt)
                .Skip(skip)
                .Take(take)
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

            return Ok(new
            {
                totalCount,
                count = apps.Count,
                skip,
                take,
                data = apps
            });
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
        /// Get all system users with pagination
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            // ✅ FIXED: Add pagination
            if (skip < 0 || take < 1 || take > 500)
            {
                return BadRequest(new { message = "Invalid pagination parameters" });
            }

            var totalCount = await _context.Users.CountAsync();

            var users = await _context.Users
                .Include(u => u.Role)
                .OrderBy(u => u.Id)
                .Skip(skip)
                .Take(take)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    Role = u.Role.Name
                })
                .ToListAsync();

            return Ok(new
            {
                totalCount,
                count = users.Count,
                skip,
                take,
                data = users
            });
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
        /// Get all applications (technical and non-technical) with pagination
        /// </summary>
        [HttpGet("all-applications")]
        public async Task<IActionResult> GetAllApplicationsUnfiltered([FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            // ✅ FIXED: Add pagination
            if (skip < 0 || take < 1 || take > 500)
            {
                return BadRequest(new { message = "Invalid pagination parameters" });
            }

            var totalCount = await _context.Applications.CountAsync();

            var apps = await _context.Applications
                .Include(a => a.RoleApplied)
                .Include(a => a.Applicant)
                .OrderByDescending(a => a.CreatedAt)
                .Skip(skip)
                .Take(take)
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

            return Ok(new
            {
                totalCount,
                count = apps.Count,
                skip,
                take,
                data = apps
            });
        }
    }
}