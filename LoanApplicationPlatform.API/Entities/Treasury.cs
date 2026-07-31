using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanApplicationPlatform.API.Entities
{
    public class Treasury : ITenantEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public decimal Balance { get; set; }

        [ForeignKey("Tenant")]
        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
