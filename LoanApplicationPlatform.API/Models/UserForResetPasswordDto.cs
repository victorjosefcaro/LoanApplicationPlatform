using System.ComponentModel.DataAnnotations;

namespace LoanApplicationPlatform.API.Models
{
    public class UserForResetPasswordDto
    {
        [Required]
        public string? Password { get; set; }
    }
}
