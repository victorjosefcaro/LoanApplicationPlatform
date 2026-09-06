using LoanApplicationPlatform.API.Helpers;
using LoanApplicationPlatform.API.Models;
using LoanApplicationPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanApplicationPlatform.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetTeamMembers([FromQuery] UserResourceParameters parameters)
        {
            var pagedUsers = await _userService.GetTeamMembersAsync(parameters);

            var paginationMetadata = new
            {
                totalCount = pagedUsers.TotalCount,
                pageSize = pagedUsers.PageSize,
                currentPage = pagedUsers.CurrentPage,
                totalPages = pagedUsers.TotalPages,
                hasPrevious = pagedUsers.HasPrevious,
                hasNext = pagedUsers.HasNext
            };

            Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(paginationMetadata));

            return Ok(pagedUsers);
        }

        [HttpPut("{id}/role")]
        public async Task<ActionResult> UpdateRole(int id, UserForUpdateRoleDto dto)
        {
            var callerId = GetCallerId();
            if (callerId == null) return Unauthorized();

            var (success, errorMessage, notFound) = await _userService.UpdateRoleAsync(callerId.Value, id, dto);
            if (notFound) return NotFound(errorMessage);
            if (!success) return BadRequest(errorMessage);
            return Ok();
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateStatus(int id, UserForStatusUpdateDto dto)
        {
            var callerId = GetCallerId();
            if (callerId == null) return Unauthorized();

            var (success, errorMessage, notFound) = await _userService.SetActiveAsync(callerId.Value, id, dto.IsActive);
            if (notFound) return NotFound(errorMessage);
            if (!success) return BadRequest(errorMessage);
            return Ok();
        }

        [HttpPut("{id}/password")]
        public async Task<ActionResult> ResetPassword(int id, UserForResetPasswordDto dto)
        {
            var (success, errorMessage, notFound) = await _userService.ResetPasswordAsync(id, dto);
            if (notFound) return NotFound(errorMessage);
            if (!success) return BadRequest(errorMessage);
            return Ok();
        }

        private int? GetCallerId()
        {
            var callerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(callerIdStr, out var callerId) ? callerId : null;
        }
    }
}
