using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LoanApplicationPlatform.API.Constants;

namespace LoanApplicationPlatform.API.Entities
{
    public class PaymentSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int LoanApplicationId { get; set; }
        
        [ForeignKey("LoanApplicationId")]
        public LoanApplication? LoanApplication { get; set; }

        public DateTime DueDate { get; set; }
        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    }
}
