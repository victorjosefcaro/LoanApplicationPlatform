using System.ComponentModel.DataAnnotations;

namespace LoanApplicationPlatform.API.Models
{
    public class ApproveDto
    {
        [Required]
        [RegularExpression("^(Approved|Rejected|Returned)$", ErrorMessage = "Status must be Approved, Rejected, or Returned.")]
        public string Status { get; set; } = string.Empty;

        public string? Remarks { get; set; }
    }
}
