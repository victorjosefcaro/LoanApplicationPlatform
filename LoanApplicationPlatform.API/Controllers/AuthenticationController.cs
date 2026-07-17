using LoanApplicationPlatform.API.DbContexts;
using LoanApplicationPlatform.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoanApplicationPlatform.API.Controllers
{
    [ApiController]
    [Route("api/authentication")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly LoanApplicationPlatformContext _context;

        public class AuthenticationRequestBody
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        public class AdminRegistrationRequestBody
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? Role { get; set; }
        }

        public class ApplicantRegistrationRequestBody
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        public AuthenticationController(IConfiguration configuration, LoanApplicationPlatformContext context)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<string>> Authenticate(AuthenticationRequestBody authenticationRequestBody)
        {
            var user = await ValidateUserCredentials(
                authenticationRequestBody.Username,
                authenticationRequestBody.Password);

            if (user == null)
            {
                return Unauthorized();
            }

            var securityKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"] ?? string.Empty));
            var signingCredentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            var claimsForToken = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                signingCredentials);

            var tokenToReturn = new JwtSecurityTokenHandler()
               .WriteToken(jwtSecurityToken);

            return Ok(tokenToReturn);
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterApplicant(ApplicantRegistrationRequestBody requestBody)
        {
            if (string.IsNullOrWhiteSpace(requestBody.Username) || string.IsNullOrWhiteSpace(requestBody.Password))
            {
                return BadRequest("Username and Password are required.");
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == requestBody.Username);
            if (existingUser != null)
            {
                return Conflict("Username already exists.");
            }

            var newUser = new User
            {
                Username = requestBody.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(requestBody.Password),
                Role = "Applicant"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("admin/register")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<ActionResult> RegisterUserAdmin(AdminRegistrationRequestBody requestBody)
        {
            if (string.IsNullOrWhiteSpace(requestBody.Username) || string.IsNullOrWhiteSpace(requestBody.Password) || string.IsNullOrWhiteSpace(requestBody.Role))
            {
                return BadRequest("Username, Password, and Role are required.");
            }

            var validRoles = new[] { "Applicant", "Reviewer", "Approver", "Admin" };
            if (!validRoles.Contains(requestBody.Role))
            {
                return BadRequest($"Invalid role. Valid roles are: {string.Join(", ", validRoles)}.");
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == requestBody.Username);
            if (existingUser != null)
            {
                return Conflict("Username already exists.");
            }

            var newUser = new User
            {
                Username = requestBody.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(requestBody.Password),
                Role = requestBody.Role
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<User?> ValidateUserCredentials(string? username, string? password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }
            return null;
        }
    }
}
