namespace BoticAPI.Services
{
    public interface IBotService
    {
        Task<(bool success, string message, int? jobId)> RunBotAsync(bool dryRun, int batchSize, string triggeredBy);
    }
}