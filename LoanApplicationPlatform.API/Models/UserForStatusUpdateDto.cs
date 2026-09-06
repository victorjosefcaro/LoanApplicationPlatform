using System.ComponentModel.DataAnnotations;

namespace LoanApplicationPlatform.API.Models
{
    public class UserForStatusUpdateDto
    {
        [Required]
        public bool IsActive { get; set; }
    }
}
