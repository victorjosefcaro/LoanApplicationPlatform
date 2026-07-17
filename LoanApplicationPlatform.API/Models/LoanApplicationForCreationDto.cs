using System.ComponentModel.DataAnnotations;

namespace LoanApplicationPlatform.API.Models
{
    public class LoanApplicationForCreationDto
    {
        [Required]
        [MaxLength(100)]
        public string ApplicantName { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public int TermInMonths { get; set; }

        [Required]
        public decimal MonthlyIncome { get; set; }

        [Required]
        [MaxLength(500)]
        public string Purpose { get; set; } = string.Empty;
    }
}
