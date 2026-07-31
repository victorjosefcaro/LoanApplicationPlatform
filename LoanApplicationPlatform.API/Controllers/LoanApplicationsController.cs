using LoanApplicationPlatform.API.Helpers;
using LoanApplicationPlatform.API.Models;
using LoanApplicationPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanApplicationPlatform.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/loanapplications")]
    public class LoanApplicationsController : ControllerBase
    {
        private readonly ILoanApplicationService _loanApplicationService;

        public LoanApplicationsController(ILoanApplicationService loanApplicationService)
        {
            _loanApplicationService = loanApplicationService ?? throw new ArgumentNullException(nameof(loanApplicationService));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoanApplicationDto>>> GetApplications([FromQuery] ResourceParameters parameters)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userIdStr == null || role == null) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var pagedApplications = await _loanApplicationService.GetApplicationsAsync(userId, role, parameters);

            var paginationMetadata = new
            {
                totalCount = pagedApplications.TotalCount,
                pageSize = pagedApplications.PageSize,
                currentPage = pagedApplications.CurrentPage,
                totalPages = pagedApplications.TotalPages,
                hasPrevious = pagedApplications.HasPrevious,
                hasNext = pagedApplications.HasNext
            };

            Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(paginationMetadata));

            return Ok(pagedApplications);
        }

        [HttpGet("{id}", Name = "GetApplication")]
        public async Task<ActionResult<LoanApplicationDto>> GetApplication(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userIdStr == null || role == null) return Unauthorized();

            var (dto, errorMessage, notFound, forbid) = await _loanApplicationService.GetApplicationAsync(id, int.Parse(userIdStr), role);
            if (notFound) return NotFound();
            if (forbid) return Forbid();

            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Applicant")]
        public async Task<ActionResult<LoanApplicationDto>> CreateApplication([FromBody] LoanApplicationForCreationDto applicationDto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();

            var (createdDto, errorMessage) = await _loanApplicationService.CreateApplicationAsync(int.Parse(userIdStr), applicationDto);
            if (errorMessage != null)
            {
                return BadRequest(errorMessage);
            }

            return CreatedAtRoute("GetApplication", new { id = createdDto!.Id }, createdDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Applicant")]
        public async Task<ActionResult> UpdateApplication(int id, [FromBody] LoanApplicationForUpdateDto applicationDto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();

            var (success, errorMessage, notFound, forbid) = await _loanApplicationService.UpdateApplicationAsync(id, int.Parse(userIdStr), applicationDto);
            if (notFound) return NotFound();
            if (forbid) return Forbid();
            if (!success) return BadRequest(errorMessage);

            return NoContent();
        }

        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "Applicant")]
        public async Task<ActionResult> CancelApplication(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();

            var (success, errorMessage, notFound, forbid) = await _loanApplicationService.CancelApplicationAsync(id, int.Parse(userIdStr));
            if (notFound) return NotFound();
            if (forbid) return Forbid();
            if (!success) return BadRequest(errorMessage);

            return NoContent();
        }

        [HttpPatch("{id}/review")]
        [Authorize(Policy = "RequireReviewerRole")]
        public async Task<ActionResult> ReviewApplication(int id, [FromBody] ReviewDto reviewDto)
        {
            var (success, errorMessage, notFound) = await _loanApplicationService.ReviewApplicationAsync(id, reviewDto);
            if (notFound) return NotFound();
            if (!success) return BadRequest(errorMessage);

            return NoContent();
        }

        [HttpPatch("{id}/approve")]
        [Authorize(Policy = "RequireApproverRole")]
        public async Task<ActionResult> ApproveApplication(int id, [FromBody] ApproveDto approveDto)
        {
            var (success, errorMessage, notFound) = await _loanApplicationService.ApproveApplicationAsync(id, approveDto);
            if (notFound) return NotFound();
            if (!success) return BadRequest(errorMessage);

            return NoContent();
        }

        [HttpPost("{id}/release")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ReleaseFunds(int id)
        {
            var (success, errorMessage, notFound) = await _loanApplicationService.ReleaseFundsAsync(id);
            if (notFound) return NotFound();
            if (!success) return BadRequest(errorMessage);

            return NoContent();
        }
    }
}
