namespace BoticAPI.DTOs
{
    // ===== Auth Request Models =====
    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegisterRequest
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int RoleId { get; set; }
    }

    // ===== Application Request Models =====
    public class CreateApplicationRequest
    {
        public string RoleName { get; set; } = null!;
    }

    public class UpdateStatusRequest
    {
        public string NewStatus { get; set; } = null!;
        public string? Comment { get; set; }
    }

    // ===== Bot Request Models =====
    public class BotRunRequest
    {
        public bool DryRun { get; set; } = false;
        public int BatchSize { get; set; } = 50;
    }

    // ===== Admin Request Models =====
    public class CreateRoleRequest
    {
        public string Name { get; set; } = null!;
        public bool IsTechnical { get; set; }
    }
}