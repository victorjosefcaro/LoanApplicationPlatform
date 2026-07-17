using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanApplicationPlatform.API.Entities
{
    public class LoanApplication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        public int ApplicantId { get; set; }
        
        [ForeignKey("ApplicantId")]
        public User? Applicant { get; set; }

        [Required]
        [MaxLength(100)]
        public string ApplicantName { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public int TermInMonths { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal InterestRate { get; set; }

        [MaxLength(500)]
        public string Purpose { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Under Review, Approved, Rejected, Released, Cancelled
        
        [MaxLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PaymentSchedule> PaymentSchedules { get; set; } = new List<PaymentSchedule>();
    }
}
