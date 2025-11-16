using BoticAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BoticAPI.Data
{
    public class BoticDbContext : DbContext
    {
        public BoticDbContext(DbContextOptions<BoticDbContext> options)
            : base(options)
        {
        }

        // DbSet properties - These are REQUIRED
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Application> Applications { get; set; } = null!;
        public DbSet<ActivityLog> ActivityLogs { get; set; } = null!;
        public DbSet<BotJob> BotJobs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Role Configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(320);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Application Configuration
            modelBuilder.Entity<Application>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CurrentStatus).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BotLockToken).HasMaxLength(50);
                entity.Property<uint>("xmin").IsRowVersion();

                entity.HasOne(e => e.Applicant)
                    .WithMany(u => u.Applications)
                    .HasForeignKey(e => e.ApplicantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RoleApplied)
                    .WithMany(r => r.Applications)
                    .HasForeignKey(e => e.RoleAppliedId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.BotLockToken);
                entity.HasIndex(e => e.LastBotRunAt);
                entity.HasIndex(e => e.RoleAppliedId);
                entity.HasIndex(e => e.ApplicantId);
            });

            // ActivityLog Configuration
            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OldStatus).HasMaxLength(100);
                entity.Property(e => e.NewStatus).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).IsRequired().HasMaxLength(320);
                entity.Property(e => e.UpdatedByRole).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.Application)
                    .WithMany(a => a.ActivityLogs)
                    .HasForeignKey(e => e.ApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.ApplicationId);
            });

            // BotJob Configuration
            modelBuilder.Entity<BotJob>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TriggeredBy).IsRequired().HasMaxLength(320);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.TriggeredAt);
            });
        }
    }

    public static class SeedData
    {
        public static void Initialize(BoticDbContext context)
        {
            // Check if database is already seeded
            if (context.Roles.Any())
            {
                return;
            }

            // Create roles
            var roles = new List<Role>
            {
                new Role { Name = "Applicant", IsTechnical = false },
                new Role { Name = "Bot", IsTechnical = false },
                new Role { Name = "Admin", IsTechnical = false },
                new Role { Name = "Backend Engineer", IsTechnical = true },
                new Role { Name = "Frontend Developer", IsTechnical = true },
                new Role { Name = "DevOps Engineer", IsTechnical = true },
                new Role { Name = "Sales Associate", IsTechnical = false },
                new Role { Name = "HR Manager", IsTechnical = false }
            };

            context.Roles.AddRange(roles);
            context.SaveChanges();

            // Get role IDs for seeding users
            var adminRole = context.Roles.First(r => r.Name == "Admin");
            var botRole = context.Roles.First(r => r.Name == "Bot");
            var applicantRole = context.Roles.First(r => r.Name == "Applicant");

            // Create users
            var users = new List<User>
            {
                new User
                {
                    Name = "Admin User",
                    Email = "admin@botic.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    RoleId = adminRole.Id
                },
                new User
                {
                    Name = "Bot User",
                    Email = "bot@botic.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Bot@123"),
                    RoleId = botRole.Id
                },
                new User
                {
                    Name = "Applicant One",
                    Email = "applicant@botic.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Applicant@123"),
                    RoleId = applicantRole.Id
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}