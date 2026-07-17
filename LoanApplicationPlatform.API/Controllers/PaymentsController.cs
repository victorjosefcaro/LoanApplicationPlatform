using AutoMapper;
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

        [HttpPost("{scheduleId}/pay")]
        [Authorize(Roles = "Applicant")]
        public async Task<ActionResult> SubmitPayment(int loanApplicationId, int scheduleId, [FromBody] PaymentDto paymentDto)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(loanApplicationId);
            if (application == null) return NotFound();

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null || application.ApplicantId != int.Parse(userIdStr)) return Forbid();

            var schedules = await _loanRepository.GetPaymentSchedulesAsync(loanApplicationId);
            var scheduleToPay = schedules.FirstOrDefault(s => s.Id == scheduleId);

            if (scheduleToPay == null) return NotFound("Payment schedule not found.");
            
            if (scheduleToPay.Status == "Paid") return BadRequest("This schedule is already paid.");

            if (paymentDto.Amount > scheduleToPay.AmountDue - scheduleToPay.AmountPaid)
            {
                return BadRequest("Payment amount exceeds the remaining balance for this schedule.");
            }

            scheduleToPay.AmountPaid += paymentDto.Amount;
            
            if (scheduleToPay.AmountPaid >= scheduleToPay.AmountDue)
            {
                scheduleToPay.Status = "Paid";
            }
            else
            {
                scheduleToPay.Status = "Partially Paid";
            }

            // Update Treasury
            var treasury = await _loanRepository.GetTreasuryAsync();
            if (treasury != null)
            {
                treasury.Balance += paymentDto.Amount;
            }

            await _loanRepository.SaveChangesAsync();
            return NoContent();
        }
    }
}
