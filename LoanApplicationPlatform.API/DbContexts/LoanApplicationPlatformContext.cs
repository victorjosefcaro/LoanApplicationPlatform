using Microsoft.EntityFrameworkCore;
using LoanApplicationPlatform.API.Constants;
using LoanApplicationPlatform.API.Entities;
using LoanApplicationPlatform.API.Services;

namespace LoanApplicationPlatform.API.DbContexts
{
    public class LoanApplicationPlatformContext : DbContext
    {
        private readonly ITenantService _tenantService;

        public DbSet<Tenant> Tenants { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<LoanApplication> LoanApplications { get; set; } = null!;
        public DbSet<PaymentSchedule> PaymentSchedules { get; set; } = null!;
        public DbSet<Treasury> Treasury { get; set; } = null!;
        public DbSet<TreasuryTransaction> TreasuryTransactions { get; set; } = null!;

        public LoanApplicationPlatformContext(
            DbContextOptions<LoanApplicationPlatformContext> options,
            ITenantService tenantService)
            : base(options)
        {
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
            configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal column types to eliminate truncation warnings
            modelBuilder.Entity<LoanApplication>(entity =>
            {
                entity.Property(l => l.Amount).HasColumnType("decimal(18,2)");
                entity.Property(l => l.InterestRate).HasColumnType("decimal(18,2)");
                entity.Property(l => l.MonthlyIncome).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<PaymentSchedule>(entity =>
            {
                entity.Property(p => p.AmountDue).HasColumnType("decimal(18,2)");
                entity.Property(p => p.AmountPaid).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Treasury>(entity =>
            {
                entity.Property(t => t.Balance).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<TreasuryTransaction>(entity =>
            {
                entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            });

            // Store enums as strings in the database for readability
            modelBuilder.Entity<LoanApplication>()
                .Property(l => l.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<PaymentSchedule>()
                .Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Prevent multiple cascade paths to the same table (SQL Server limitation).
            // TenantId FKs use Restrict since tenant deletion is an administrative action
            // that should be handled explicitly, not cascaded automatically.
            modelBuilder.Entity<User>()
                .HasOne(u => u.Tenant)
                .WithMany()
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanApplication>()
                .HasOne(l => l.Tenant)
                .WithMany()
                .HasForeignKey(l => l.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanApplication>()
                .HasOne(l => l.Applicant)
                .WithMany()
                .HasForeignKey(l => l.ApplicantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentSchedule>()
                .HasOne(p => p.Tenant)
                .WithMany()
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Treasury>()
                .HasOne(t => t.Tenant)
                .WithMany()
                .HasForeignKey(t => t.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TreasuryTransaction>()
                .HasOne(t => t.Tenant)
                .WithMany()
                .HasForeignKey(t => t.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Global Query Filters for Multitenancy
            modelBuilder.Entity<User>()
                .HasQueryFilter(u => u.TenantId == _tenantService.GetCurrentTenantId());

            modelBuilder.Entity<LoanApplication>()
                .HasQueryFilter(l => l.TenantId == _tenantService.GetCurrentTenantId());

            modelBuilder.Entity<PaymentSchedule>()
                .HasQueryFilter(p => p.TenantId == _tenantService.GetCurrentTenantId());

            modelBuilder.Entity<Treasury>()
                .HasQueryFilter(t => t.TenantId == _tenantService.GetCurrentTenantId());

            modelBuilder.Entity<TreasuryTransaction>()
                .HasQueryFilter(t => t.TenantId == _tenantService.GetCurrentTenantId());

            // Seed Tenants
            modelBuilder.Entity<Tenant>().HasData(
                new Tenant { Id = 1, Name = "Tenant 1" },
                new Tenant { Id = 2, Name = "Tenant 2" }
            );

            // Seed initial Treasuries
            modelBuilder.Entity<Treasury>().HasData(
                new Treasury { Id = 1, Balance = 1000000m, TenantId = 1 },
                new Treasury { Id = 2, Balance = 2000000m, TenantId = 2 }
            );

            // BCrypt hash for "password123"
            const string defaultPasswordHash = "$2a$11$0IAZiPtgMtoVLj7FFEC4YewGS1HZeelrFS66dmExWVaeeirTLHjey";

            // Seed Users for Tenants
            modelBuilder.Entity<User>().HasData(
                // Tenant 1 Users
                new User { Id = 1, Username = "t1_admin",     PasswordHash = defaultPasswordHash, Role = "Admin",     TenantId = 1 },
                new User { Id = 2, Username = "t1_applicant", PasswordHash = defaultPasswordHash, Role = "Applicant", TenantId = 1 },
                new User { Id = 3, Username = "t1_reviewer",  PasswordHash = defaultPasswordHash, Role = "Reviewer",  TenantId = 1 },
                new User { Id = 4, Username = "t1_approver",  PasswordHash = defaultPasswordHash, Role = "Approver",  TenantId = 1 },

                // Tenant 2 Users
                new User { Id = 5, Username = "t2_admin",     PasswordHash = defaultPasswordHash, Role = "Admin",     TenantId = 2 },
                new User { Id = 6, Username = "t2_applicant", PasswordHash = defaultPasswordHash, Role = "Applicant", TenantId = 2 },
                new User { Id = 7, Username = "t2_reviewer",  PasswordHash = defaultPasswordHash, Role = "Reviewer",  TenantId = 2 },
                new User { Id = 8, Username = "t2_approver",  PasswordHash = defaultPasswordHash, Role = "Approver",  TenantId = 2 }
            );
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            int currentTenantId = _tenantService.GetCurrentTenantId();

            foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity.TenantId == 0)
                    {
                        entry.Entity.TenantId = currentTenantId;
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
