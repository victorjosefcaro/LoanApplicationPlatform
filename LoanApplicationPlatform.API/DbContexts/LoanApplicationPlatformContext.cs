using Microsoft.EntityFrameworkCore;
using LoanApplicationPlatform.API.Constants;
using LoanApplicationPlatform.API.Entities;

namespace LoanApplicationPlatform.API.DbContexts
{
    public class LoanApplicationPlatformContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<LoanApplication> LoanApplications { get; set; } = null!;
        public DbSet<PaymentSchedule> PaymentSchedules { get; set; } = null!;
        public DbSet<Treasury> Treasury { get; set; }
        public DbSet<TreasuryTransaction> TreasuryTransactions { get; set; } = null!;

        public LoanApplicationPlatformContext(DbContextOptions<LoanApplicationPlatformContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Store enums as strings in the database for readability
            modelBuilder.Entity<LoanApplication>()
                .Property(l => l.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<PaymentSchedule>()
                .Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Seed initial data
            modelBuilder.Entity<Treasury>().HasData(
                new Treasury { Id = 1, Balance = 1000000m } // Start with 1M in the treasury
            );

            // Seed Admin User (Using a real BCrypt hash for "password")
            modelBuilder.Entity<User>().HasData(
                // Pre-computed BCrypt hash for "password" — avoids migration churn from random salt generation
                new User { Id = 1, Username = "admin", PasswordHash = "$2a$11$3ieT9rszDmCDFAmvST.IE.CBESY005xlEuNWBhleQUQNlA2kKHMV.", Role = "Admin" }
            );
        }
    }
}
