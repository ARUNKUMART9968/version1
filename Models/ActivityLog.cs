namespace BoticAPI.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string? OldStatus { get; set; }
        public string NewStatus { get; set; } = null!;
        public string UpdatedBy { get; set; } = null!;
        public string UpdatedByRole { get; set; } = null!;
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Application Application { get; set; } = null!;
    }
}