namespace BoticAPI.Models
{
    public class BotJob
    {
        public int Id { get; set; }
        public string TriggeredBy { get; set; } = null!;
        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Running";
        public int? TotalProcessed { get; set; }
        public int? TotalSucceeded { get; set; }
        public int? TotalFailed { get; set; }
        public string? Details { get; set; }
    }
}