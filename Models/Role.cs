namespace BoticAPI.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsTechnical { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}