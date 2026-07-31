using LoanApplicationPlatform.API.Models;
using LoanApplicationPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplicationPlatform.API.Controllers
{
    [ApiController]
    [Route("api/authentication")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthenticationController(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Authenticate(LoginRequestDto loginRequest)
        {
            var token = await _authService.AuthenticateAsync(loginRequest);
            if (token == null)
            {
                return Unauthorized();
            }

            return Ok(token);
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterApplicant(LoginRequestDto requestBody)
        {
            var (success, errorMessage) = await _authService.RegisterApplicantAsync(requestBody);
            if (!success)
            {
                if (errorMessage == "Username already exists.") return Conflict(errorMessage);
                return BadRequest(errorMessage);
            }

            return Ok();
        }
        [HttpPost("admin/register")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult> RegisterUserAdmin(AdminRegistrationDto requestBody)
        {
            var (success, errorMessage) = await _authService.RegisterAdminUserAsync(requestBody);
            if (!success)
            {
                if (errorMessage == "Username already exists.") return Conflict(errorMessage);
                return BadRequest(errorMessage);
            }

            return Ok();
        }
    }
}
