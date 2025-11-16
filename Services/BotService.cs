using BoticAPI.Data;
using BoticAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BoticAPI.Services
{
    public class BotService : IBotService
    {
        private readonly BoticDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string[] _statusFlow = { "Applied", "Reviewed", "CodingRound", "TechnicalInterview", "HRInterview", "Offer", "Hired" };

        public BotService(BoticDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(bool success, string message, int? jobId)> RunBotAsync(bool dryRun, int batchSize, string triggeredBy)
        {
            var botJob = new BotJob
            {
                TriggeredBy = triggeredBy,
                Status = "Running"
            };

            _context.BotJobs.Add(botJob);
            await _context.SaveChangesAsync();

            try
            {
                var minSecondsInStageConfig = _configuration["Bot:MinSecondsInStage"];
                if (!int.TryParse(minSecondsInStageConfig, out var minSecondsInStage))
                {
                    throw new InvalidOperationException("Bot:MinSecondsInStage configuration is invalid");
                }

                var threshold = DateTime.UtcNow.AddSeconds(-minSecondsInStage);

                var eligibleApps = await _context.Applications
                    .Where(a => a.RoleApplied.IsTechnical &&
                               a.BotLockToken == null &&
                               a.CurrentStatus != "Hired" &&
                               a.CurrentStatus != "Offer" &&
                               (a.LastBotRunAt == null || a.LastBotRunAt < threshold))
                    .Include(a => a.RoleApplied)
                    .Take(batchSize)
                    .ToListAsync();

                int succeeded = 0, failed = 0;

                foreach (var app in eligibleApps)
                {
                    try
                    {
                        using var tx = await _context.Database.BeginTransactionAsync();

                        // Lock application
                        app.BotLockToken = Guid.NewGuid().ToString();
                        _context.Applications.Update(app);
                        await _context.SaveChangesAsync();

                        var nextStatus = GetNextStatus(app.CurrentStatus);

                        if (!dryRun && nextStatus != null)
                        {
                            var oldStatus = app.CurrentStatus;
                            app.CurrentStatus = nextStatus;
                            app.LastBotRunAt = DateTime.UtcNow;

                            var activityLog = new ActivityLog
                            {
                                ApplicationId = app.Id,
                                OldStatus = oldStatus,
                                NewStatus = nextStatus,
                                UpdatedBy = "bot@botic.local",
                                UpdatedByRole = "Bot",
                                Comment = $"Automated transition from {oldStatus} to {nextStatus}"
                            };

                            _context.ActivityLogs.Add(activityLog);
                            _context.Applications.Update(app);
                            await _context.SaveChangesAsync();
                        }

                        // Unlock application
                        app.BotLockToken = null;
                        _context.Applications.Update(app);
                        await _context.SaveChangesAsync();

                        await tx.CommitAsync();
                        succeeded++;
                    }
                    catch (Exception)
                    {
                        failed++;
                        app.BotLockToken = null;
                        _context.Applications.Update(app);
                        await _context.SaveChangesAsync();
                    }
                }

                botJob.Status = "Completed";
                botJob.TotalProcessed = eligibleApps.Count;
                botJob.TotalSucceeded = succeeded;
                botJob.TotalFailed = failed;
                botJob.Details = $"Processed {eligibleApps.Count} applications. Succeeded: {succeeded}, Failed: {failed}";

                _context.BotJobs.Update(botJob);
                await _context.SaveChangesAsync();

                return (true, $"Bot completed. Processed: {eligibleApps.Count}, Succeeded: {succeeded}, Failed: {failed}", botJob.Id);
            }
            catch (Exception ex)
            {
                botJob.Status = "Failed";
                botJob.Details = ex.Message;
                _context.BotJobs.Update(botJob);
                await _context.SaveChangesAsync();

                return (false, $"Bot run failed: {ex.Message}", botJob.Id);
            }
        }

        private string? GetNextStatus(string currentStatus)
        {
            var currentIndex = Array.IndexOf(_statusFlow, currentStatus);
            if (currentIndex < 0 || currentIndex >= _statusFlow.Length - 1)
            {
                return null;
            }

            return _statusFlow[currentIndex + 1];
        }
    }
}