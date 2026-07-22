using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanApplicationPlatform.API.Entities
{
    public class TreasuryTransaction : ITenantEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime TransactionDate { get; set; }

        public decimal Amount { get; set; }

        public string Type { get; set; } = string.Empty;

        public int? ReferenceId { get; set; }

        [ForeignKey("Tenant")]
        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
