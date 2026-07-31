using System.ComponentModel.DataAnnotations;

namespace LoanApplicationPlatform.API.Models
{
    public class DepositDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Deposit amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }
}
