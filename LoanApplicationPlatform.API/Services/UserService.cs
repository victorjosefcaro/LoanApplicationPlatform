using AutoMapper;
using LoanApplicationPlatform.API.Helpers;
using LoanApplicationPlatform.API.Models;
using LoanApplicationPlatform.API.Repositories;

namespace LoanApplicationPlatform.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        // Roles a team member can hold. Applicant is deliberately excluded — this
        // surface manages staff accounts only.
        private static readonly string[] StaffRoles = { "Admin", "Reviewer", "Approver" };

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PagedList<UserDto>> GetTeamMembersAsync(UserResourceParameters parameters)
        {
            var users = await _userRepository.GetTeamMembersAsync(parameters);
            var dtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return new PagedList<UserDto>(dtos.ToList(), users.TotalCount, users.CurrentPage, users.PageSize);
        }

        public async Task<(bool Success, string? ErrorMessage, bool NotFound)> UpdateRoleAsync(int callerId, int id, UserForUpdateRoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Role) || !StaffRoles.Contains(dto.Role))
            {
                return (false, $"Invalid role. Valid roles are: {string.Join(", ", StaffRoles)}.", false);
            }

            if (id == callerId)
            {
                return (false, "You cannot change your own role.", false);
            }

            // Tenant-scoped: a user from another tenant resolves to null here.
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.Role == "Applicant")
            {
                return (false, "Team member not found.", true);
            }

            user.Role = dto.Role;
            await _userRepository.SaveChangesAsync();
            return (true, null, false);
        }

        public async Task<(bool Success, string? ErrorMessage, bool NotFound)> SetActiveAsync(int callerId, int id, bool isActive)
        {
            if (id == callerId && !isActive)
            {
                return (false, "You cannot deactivate your own account.", false);
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.Role == "Applicant")
            {
                return (false, "Team member not found.", true);
            }

            user.IsActive = isActive;
            await _userRepository.SaveChangesAsync();
            return (true, null, false);
        }

        public async Task<(bool Success, string? ErrorMessage, bool NotFound)> ResetPasswordAsync(int id, UserForResetPasswordDto dto)
        {
            var (isPasswordValid, passwordError) = UserValidation.ValidatePassword(dto.Password ?? string.Empty);
            if (!isPasswordValid)
            {
                return (false, passwordError, false);
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.Role == "Applicant")
            {
                return (false, "Team member not found.", true);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            await _userRepository.SaveChangesAsync();
            return (true, null, false);
        }
    }
}
