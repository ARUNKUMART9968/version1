using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BoticAPI.Services;
using BoticAPI.Data;
using BoticAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BoticAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BotController : ControllerBase
    {
        private readonly IBotService _botService;
        private readonly BoticDbContext _context;

        public BotController(IBotService botService, BoticDbContext context)
        {
            _botService = botService;
            _context = context;
        }

        private string GetUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        }

        /// <summary>
        /// Trigger bot to process technical role applications
        /// </summary>
        [HttpPost("run")]
        [Authorize(Roles = "Admin,Bot")]
        public async Task<IActionResult> RunBot([FromBody] BotRunRequest request)
        {
            var userEmail = GetUserEmail();
            var (success, message, jobId) = await _botService.RunBotAsync(request.DryRun, request.BatchSize, userEmail);

            if (!success)
            {
                return BadRequest(new { message, jobId });
            }

            return Ok(new { message, jobId, status = "Bot run initiated" });
        }

        /// <summary>
        /// Get bot job status and results
        /// </summary>
        [HttpGet("jobs/{id}")]
        public async Task<IActionResult> GetBotJob(int id)
        {
            var job = await _context.BotJobs.FirstOrDefaultAsync(j => j.Id == id);
            if (job == null)
            {
                return NotFound(new { message = "Bot job not found" });
            }

            return Ok(new
            {
                job.Id,
                job.Status,
                job.TotalProcessed,
                job.TotalSucceeded,
                job.TotalFailed,
                job.TriggeredAt,
                job.TriggeredBy,
                job.Details
            });
        }

        /// <summary>
        /// Get recent bot jobs
        /// </summary>
        [HttpGet("jobs")]
        [Authorize(Roles = "Admin,Bot")]
        public async Task<IActionResult> GetRecentBotJobs(int limit = 10)
        {
            var jobs = await _context.BotJobs
                .OrderByDescending(j => j.TriggeredAt)
                .Take(limit)
                .Select(j => new
                {
                    j.Id,
                    j.Status,
                    j.TotalProcessed,
                    j.TotalSucceeded,
                    j.TotalFailed,
                    j.TriggeredAt,
                    j.TriggeredBy
                })
                .ToListAsync();

            return Ok(jobs);
        }
    }
}