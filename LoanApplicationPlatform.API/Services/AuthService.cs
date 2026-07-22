using LoanApplicationPlatform.API.Entities;
using LoanApplicationPlatform.API.Models;
using LoanApplicationPlatform.API.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoanApplicationPlatform.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<string?> AuthenticateAsync(LoginRequestDto loginRequest)
        {
            if (string.IsNullOrWhiteSpace(loginRequest.Username) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return null;
            }

            var user = await _userRepository.GetByUsernameAsync(loginRequest.Username, ignoreQueryFilters: true);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                return null;
            }

            var securityKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"] ?? string.Empty));
            var signingCredentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            var claimsForToken = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("tenant_id", user.TenantId.ToString())
            };

            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterApplicantAsync(LoginRequestDto requestBody)
        {
            if (string.IsNullOrWhiteSpace(requestBody.Username) || string.IsNullOrWhiteSpace(requestBody.Password))
            {
                return (false, "Username and Password are required.");
            }

            var existingUser = await _userRepository.GetByUsernameAsync(requestBody.Username, ignoreQueryFilters: true);
            if (existingUser != null)
            {
                return (false, "Username already exists.");
            }

            var newUser = new User
            {
                Username = requestBody.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(requestBody.Password),
                Role = "Applicant"
            };

            _userRepository.AddUser(newUser);
            await _userRepository.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterAdminUserAsync(AdminRegistrationDto requestBody)
        {
            if (string.IsNullOrWhiteSpace(requestBody.Username) || string.IsNullOrWhiteSpace(requestBody.Password) || string.IsNullOrWhiteSpace(requestBody.Role))
            {
                return (false, "Username, Password, and Role are required.");
            }

            var validRoles = new[] { "Applicant", "Reviewer", "Approver", "Admin" };
            if (!validRoles.Contains(requestBody.Role))
            {
                return (false, $"Invalid role. Valid roles are: {string.Join(", ", validRoles)}.");
            }

            var existingUser = await _userRepository.GetByUsernameAsync(requestBody.Username, ignoreQueryFilters: true);
            if (existingUser != null)
            {
                return (false, "Username already exists.");
            }

            var newUser = new User
            {
                Username = requestBody.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(requestBody.Password),
                Role = requestBody.Role
            };

            _userRepository.AddUser(newUser);
            await _userRepository.SaveChangesAsync();

            return (true, null);
        }
    }
}
