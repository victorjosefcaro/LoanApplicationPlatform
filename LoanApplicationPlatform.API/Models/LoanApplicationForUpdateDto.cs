using System.ComponentModel.DataAnnotations;

namespace LoanApplicationPlatform.API.Models
{
    public class LoanApplicationForUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string ApplicantName { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        [Range(1, 360, ErrorMessage = "Term must be between 1 and 360 months.")]
        public int TermInMonths { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Monthly income must be greater than zero.")]
        public decimal MonthlyIncome { get; set; }

        [Required]
        [MaxLength(500)]
        public string Purpose { get; set; } = string.Empty;
    }
}
