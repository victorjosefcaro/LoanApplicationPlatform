using System.ComponentModel.DataAnnotations;

namespace LoanApplicationPlatform.API.Models
{
    public class AdminRegistrationDto
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public string? Role { get; set; }

        [Range(1, int.MaxValue)]
        public int? TenantId { get; set; }
    }
}
