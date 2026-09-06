using System.ComponentModel.DataAnnotations.Schema;

namespace LoanApplicationPlatform.API.Entities
{
    public class User : ITenantEntity
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Applicant, Admin, Reviewer, Approver

        // Deactivated accounts are retained but blocked from signing in.
        public bool IsActive { get; set; } = true;

        [ForeignKey("Tenant")]
        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
