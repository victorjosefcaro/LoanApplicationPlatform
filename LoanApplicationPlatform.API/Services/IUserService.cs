using LoanApplicationPlatform.API.Helpers;
using LoanApplicationPlatform.API.Models;

namespace LoanApplicationPlatform.API.Services
{
    public interface IUserService
    {
        Task<PagedList<UserDto>> GetTeamMembersAsync(UserResourceParameters parameters);
        Task<(bool Success, string? ErrorMessage, bool NotFound)> UpdateRoleAsync(int callerId, int id, UserForUpdateRoleDto dto);
        Task<(bool Success, string? ErrorMessage, bool NotFound)> SetActiveAsync(int callerId, int id, bool isActive);
        Task<(bool Success, string? ErrorMessage, bool NotFound)> ResetPasswordAsync(int id, UserForResetPasswordDto dto);
    }
}
