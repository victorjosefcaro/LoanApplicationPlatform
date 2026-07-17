using Microsoft.EntityFrameworkCore;
using LoanApplicationPlatform.API.Entities;

namespace LoanApplicationPlatform.API.DbContexts
{
    public class LoanApplicationPlatformContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<LoanApplication> LoanApplications { get; set; } = null!;
        public DbSet<PaymentSchedule> PaymentSchedules { get; set; } = null!;
        public DbSet<Treasury> Treasury { get; set; } = null!;

        public LoanApplicationPlatformContext(DbContextOptions<LoanApplicationPlatformContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed initial data
            modelBuilder.Entity<Treasury>().HasData(
                new Treasury { Id = 1, Balance = 1000000m } // Start with 1M in the treasury
            );

            // Seed Admin User (We'll use a dummy hash for now)
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", PasswordHash = "dummyhash", Role = "Admin" }
            );
        }
    }
}
