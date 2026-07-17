using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Payment Submitted, Partially Paid, Paid
    }
}
