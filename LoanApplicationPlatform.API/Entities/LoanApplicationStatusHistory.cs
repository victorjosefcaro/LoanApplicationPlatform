using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LoanApplicationPlatform.API.Constants;

namespace LoanApplicationPlatform.API.Entities
{
    public class LoanApplicationStatusHistory : ITenantEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int LoanApplicationId { get; set; }

        [ForeignKey("LoanApplicationId")]
        public LoanApplication? LoanApplication { get; set; }

        public LoanStatus? PreviousStatus { get; set; }

        [Required]
        public LoanStatus NewStatus { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [Required]
        public int ChangedByUserId { get; set; }

        [ForeignKey("ChangedByUserId")]
        public User? ChangedByUser { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("Tenant")]
        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
