using AutoMapper;
using LoanApplicationPlatform.API.Constants;
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
        private readonly ILoanRepository _loanRepository;
        private readonly IMapper _mapper;

        public PaymentsController(ILoanRepository loanRepository, IMapper mapper)
        {
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentScheduleDto>>> GetPaymentSchedules(int loanApplicationId)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(loanApplicationId);
            if (application == null) return NotFound();

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userIdStr == null) return Unauthorized();

            if (role == "Applicant" && application.ApplicantId != int.Parse(userIdStr))
            {
                return Forbid();
            }

            var schedules = await _loanRepository.GetPaymentSchedulesAsync(loanApplicationId);
            return Ok(_mapper.Map<IEnumerable<PaymentScheduleDto>>(schedules));
        }

        [HttpPost("{scheduleId}/submit")]
        [Authorize(Roles = "Applicant")]
        public async Task<ActionResult> SubmitPayment(int loanApplicationId, int scheduleId)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(loanApplicationId);
            if (application == null) return NotFound();

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null || application.ApplicantId != int.Parse(userIdStr)) return Forbid();

            var schedules = await _loanRepository.GetPaymentSchedulesAsync(loanApplicationId);
            var scheduleToPay = schedules.FirstOrDefault(s => s.Id == scheduleId);

            if (scheduleToPay == null) return NotFound("Payment schedule not found.");
            
            if (scheduleToPay.Status == PaymentStatus.Paid) return BadRequest("This schedule is already paid.");

            scheduleToPay.Status = PaymentStatus.PaymentSubmitted;

            await _loanRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{scheduleId}/post")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PostPayment(int loanApplicationId, int scheduleId, [FromBody] PaymentDto paymentDto)
        {
            var schedules = await _loanRepository.GetPaymentSchedulesAsync(loanApplicationId);
            var scheduleToPost = schedules.FirstOrDefault(s => s.Id == scheduleId);

            if (scheduleToPost == null) return NotFound("Payment schedule not found.");
            
            if (scheduleToPost.Status != PaymentStatus.PaymentSubmitted && scheduleToPost.Status != PaymentStatus.PartiallyPaid)
                return BadRequest("Schedule must be in Submitted or Partially Paid status to post.");

            if (paymentDto.Amount > scheduleToPost.AmountDue - scheduleToPost.AmountPaid)
            {
                return BadRequest("Payment amount exceeds the remaining balance for this schedule.");
            }

            scheduleToPost.AmountPaid += paymentDto.Amount;
            
            if (scheduleToPost.AmountPaid >= scheduleToPost.AmountDue)
            {
                scheduleToPost.Status = PaymentStatus.Paid;
            }
            else
            {
                scheduleToPost.Status = PaymentStatus.PartiallyPaid;
            }

            // Update Treasury
            var treasury = await _loanRepository.GetTreasuryAsync();
            if (treasury != null)
            {
                treasury.Balance += paymentDto.Amount;
            }

            // Loan Closure check
            var allSchedules = await _loanRepository.GetPaymentSchedulesAsync(loanApplicationId);
            if (allSchedules.All(s => s.Status == PaymentStatus.Paid))
            {
                var application = await _loanRepository.GetLoanApplicationAsync(loanApplicationId);
                if (application != null)
                {
                    application.Status = LoanStatus.Completed;
                }
            }

            await _loanRepository.SaveChangesAsync();
            return NoContent();
        }
    }
}
