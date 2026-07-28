using LoanApplicationPlatform.API.Models;
using LoanApplicationPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanApplicationPlatform.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/loanapplications/{loanApplicationId}/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentScheduleDto>>> GetPaymentSchedules(int loanApplicationId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userIdStr == null || role == null) return Unauthorized();

            var (schedules, errorMessage, notFound, forbid) = await _paymentService.GetPaymentSchedulesAsync(loanApplicationId, int.Parse(userIdStr), role);
            if (notFound) return NotFound();
            if (forbid) return Forbid();

            return Ok(schedules);
        }

        [HttpPost("{scheduleId}/submit")]
        [Authorize(Roles = "Applicant")]
        public async Task<ActionResult> SubmitPayment(int loanApplicationId, int scheduleId, [FromBody] PaymentDto? paymentDto = null)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();

            var (success, errorMessage, notFound, forbid) = await _paymentService.SubmitPaymentAsync(loanApplicationId, scheduleId, int.Parse(userIdStr), paymentDto?.Amount);
            if (notFound) return NotFound(errorMessage);
            if (forbid) return Forbid();
            if (!success) return BadRequest(errorMessage);

            return NoContent();
        }

        [HttpPost("{scheduleId}/post")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PostPayment(int loanApplicationId, int scheduleId, [FromBody] PaymentDto paymentDto)
        {
            var (success, errorMessage, notFound) = await _paymentService.PostPaymentAsync(loanApplicationId, scheduleId, paymentDto);
            if (notFound) return NotFound(errorMessage);
            if (!success) return BadRequest(errorMessage);

            return NoContent();
        }
    }
}
