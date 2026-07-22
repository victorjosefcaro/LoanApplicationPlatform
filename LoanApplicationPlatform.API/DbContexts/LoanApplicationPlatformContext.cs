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

            // Seed Default Tenant
            modelBuilder.Entity<Tenant>().HasData(
                new Tenant { Id = 1, Name = "Default Lending Co" }
            );

            // Seed initial Treasury for Default Tenant
            modelBuilder.Entity<Treasury>().HasData(
                new Treasury { Id = 1, Balance = 1000000m, TenantId = 1 }
            );

            // Seed Admin User for Default Tenant
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "$2a$11$3ieT9rszDmCDFAmvST.IE.CBESY005xlEuNWBhleQUQNlA2kKHMV.",
                    Role = "Admin",
                    TenantId = 1
                }
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
