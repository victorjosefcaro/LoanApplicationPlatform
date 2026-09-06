using System.ComponentModel.DataAnnotations;

namespace LoanApplicationPlatform.API.Models
{
    public class UserForUpdateRoleDto
    {
        [Required]
        public string? Role { get; set; }
    }
}
