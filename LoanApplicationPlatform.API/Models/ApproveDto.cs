using System.ComponentModel.DataAnnotations;

namespace LoanApplicationPlatform.API.Models
{
    public class ApproveDto
    {
        [Required]
        [RegularExpression("^(Approved|Rejected)$", ErrorMessage = "Status must be Approved or Rejected.")]
        public string Status { get; set; } = string.Empty;

        public string? Remarks { get; set; }
    }
}
