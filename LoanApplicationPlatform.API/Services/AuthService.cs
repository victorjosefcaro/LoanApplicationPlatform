using LoanApplicationPlatform.API.Entities;
using LoanApplicationPlatform.API.DbContexts;
using LoanApplicationPlatform.API.Models;
using LoanApplicationPlatform.API.Repositories;
using Microsoft.EntityFrameworkCore;
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
        private readonly LoanApplicationPlatformContext _context;

        public AuthService(
            IUserRepository userRepository,
            IConfiguration configuration,
            LoanApplicationPlatformContext context)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(string? Token, string? ErrorMessage)> AuthenticateAsync(LoginRequestDto loginRequest)
        {
            if (string.IsNullOrWhiteSpace(loginRequest.Username) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return (null, "Username and Password are required.");
            }

            var user = await _userRepository.GetByUsernameAsync(loginRequest.Username, ignoreQueryFilters: true);
            if (user == null)
            {
                return (null, "Invalid username or password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                return (null, "Invalid Password.");
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
                DateTime.UtcNow.AddHours(2),
                signingCredentials);

            return (new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken), null);
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterApplicantAsync(LoginRequestDto requestBody)
        {
            if (string.IsNullOrWhiteSpace(requestBody.Username) || string.IsNullOrWhiteSpace(requestBody.Password))
            {
                return (false, "Username and Password are required.");
            }

            var (isPasswordValid, passwordError) = ValidatePassword(requestBody.Password);
            if (!isPasswordValid)
            {
                return (false, passwordError);
            }

            var tenantId = requestBody.TenantId ?? 1;
            if (!await _context.Tenants.AnyAsync(t => t.Id == tenantId))
            {
                return (false, "The selected tenant does not exist.");
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
                Role = "Applicant",
                TenantId = tenantId
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

            var (isPasswordValid, passwordError) = ValidatePassword(requestBody.Password);
            if (!isPasswordValid)
            {
                return (false, passwordError);
            }

            var validRoles = new[] { "Applicant", "Reviewer", "Approver", "Admin" };
            if (!validRoles.Contains(requestBody.Role))
            {
                return (false, $"Invalid role. Valid roles are: {string.Join(", ", validRoles)}.");
            }

            var tenantId = requestBody.TenantId ?? 1;
            if (!await _context.Tenants.AnyAsync(t => t.Id == tenantId))
            {
                return (false, "The selected tenant does not exist.");
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
                Role = requestBody.Role,
                TenantId = tenantId
            };

            _userRepository.AddUser(newUser);
            await _userRepository.SaveChangesAsync();

            return (true, null);
        }

        private static (bool IsValid, string? ErrorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return (false, "Password is required.");
            }

            var missingRequirements = new List<string>();

            if (password.Length < 8)
            {
                missingRequirements.Add("be at least 8 characters long");
            }

            if (!password.Any(char.IsUpper))
            {
                missingRequirements.Add("contain at least one uppercase letter");
            }

            if (!password.Any(char.IsLower))
            {
                missingRequirements.Add("contain at least one lowercase letter");
            }

            if (!password.Any(char.IsDigit))
            {
                missingRequirements.Add("contain at least one number");
            }

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                missingRequirements.Add("contain at least one special character");
            }

            if (missingRequirements.Count > 0)
            {
                return (false, $"Password must {string.Join(", ", missingRequirements)}.");
            }

            return (true, null);
        }
    }
}
