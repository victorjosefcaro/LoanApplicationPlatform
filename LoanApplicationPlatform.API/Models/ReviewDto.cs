using System.ComponentModel.DataAnnotations;

namespace LoanApplicationPlatform.API.Models
{
    public class ReviewDto
    {
        [Required]
        [RegularExpression("^(Returned|Reviewed|Rejected)$", ErrorMessage = "Status must be Returned, Reviewed, or Rejected.")]
        public string Status { get; set; } = string.Empty;

        public string? Remarks { get; set; }
    }
}
