using LoanApplicationPlatform.API.Models;

namespace LoanApplicationPlatform.API.Services
{
    public interface IAuthService
    {
        Task<string?> AuthenticateAsync(LoginRequestDto loginRequest);
        Task<(bool Success, string? ErrorMessage)> RegisterApplicantAsync(LoginRequestDto requestBody);
        Task<(bool Success, string? ErrorMessage)> RegisterAdminUserAsync(AdminRegistrationDto requestBody);
    }
}
