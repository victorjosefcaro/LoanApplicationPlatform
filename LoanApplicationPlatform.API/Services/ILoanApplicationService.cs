using LoanApplicationPlatform.API.Helpers;
using LoanApplicationPlatform.API.Models;

namespace LoanApplicationPlatform.API.Services
{
    public interface ILoanApplicationService
    {
        Task<PagedList<LoanApplicationDto>> GetApplicationsAsync(int userId, string role, ResourceParameters parameters);
        Task<(LoanApplicationDto? Dto, string? ErrorMessage, bool NotFound, bool Forbid)> GetApplicationAsync(int id, int userId, string role);
        Task<(LoanApplicationDto? Dto, string? ErrorMessage)> CreateApplicationAsync(int userId, LoanApplicationForCreationDto dto);
        Task<(bool Success, string? ErrorMessage, bool NotFound, bool Forbid)> UpdateApplicationAsync(int id, int userId, LoanApplicationForUpdateDto dto);
        Task<(bool Success, string? ErrorMessage, bool NotFound, bool Forbid)> CancelApplicationAsync(int id, int userId);
        Task<(bool Success, string? ErrorMessage, bool NotFound)> ReviewApplicationAsync(int id, ReviewDto reviewDto);
        Task<(bool Success, string? ErrorMessage, bool NotFound)> ApproveApplicationAsync(int id, ApproveDto approveDto);
        Task<(bool Success, string? ErrorMessage, bool NotFound)> ReleaseFundsAsync(int id);
    }
}
