namespace BoticAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public int RoleId { get; set; }

        public Role Role { get; set; } = null!;
        public ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}