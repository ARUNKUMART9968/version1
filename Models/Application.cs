namespace BoticAPI.Models
{
    public class Application
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public int RoleAppliedId { get; set; }
        public string CurrentStatus { get; set; } = "Applied";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastBotRunAt { get; set; }
        public string? BotLockToken { get; set; }

        public User Applicant { get; set; } = null!;
        public Role RoleApplied { get; set; } = null!;
        public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    }
}