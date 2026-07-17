using AutoMapper;
using LoanApplicationPlatform.API.Constants;
using LoanApplicationPlatform.API.Entities;
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
        private readonly ILoanRepository _loanRepository;
        private readonly IMapper _mapper;

        public LoanApplicationsController(ILoanRepository loanRepository, IMapper mapper)
        {
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoanApplicationDto>>> GetApplications()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userIdStr == null || role == null) return Unauthorized();
            int userId = int.Parse(userIdStr);

            IEnumerable<LoanApplication> applications;

            if (role == "Applicant")
            {
                applications = await _loanRepository.GetLoanApplicationsAsync(applicantId: userId);
            }
            else 
            {
                // Reviewers, Approvers, and Admins can fetch all historical applications
                // Filtering into specific buckets (Approved, Rejected, Pending) is handled by the client
                applications = await _loanRepository.GetLoanApplicationsAsync();
            }

            return Ok(_mapper.Map<IEnumerable<LoanApplicationDto>>(applications));
        }

        [HttpGet("{id}", Name = "GetApplication")]
        public async Task<ActionResult<LoanApplicationDto>> GetApplication(int id)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(id);
            if (application == null) return NotFound();

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userIdStr == null) return Unauthorized();

            if (role == "Applicant" && application.ApplicantId != int.Parse(userIdStr))
            {
                return Forbid();
            }

            return Ok(_mapper.Map<LoanApplicationDto>(application));
        }

        [HttpPost]
        [Authorize(Roles = "Applicant")]
        public async Task<ActionResult<LoanApplicationDto>> CreateApplication([FromBody] LoanApplicationForCreationDto applicationDto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();

            if (applicationDto.TermInMonths > 0)
            {
                decimal estimatedMonthlyPayment = applicationDto.Amount / applicationDto.TermInMonths;
                if (estimatedMonthlyPayment > applicationDto.MonthlyIncome)
                {
                    return BadRequest($"Submission rejected: Your monthly income ({applicationDto.MonthlyIncome:C}) is insufficient for the estimated monthly payment of {estimatedMonthlyPayment:C}.");
                }
            }

            var application = _mapper.Map<LoanApplication>(applicationDto);
            application.ApplicantId = int.Parse(userIdStr);
            application.Status = LoanStatus.Submitted;
            application.InterestRate = 0.05m; // Flat 5% interest rate
            application.CreatedAt = DateTime.UtcNow;

            _loanRepository.AddLoanApplication(application);
            await _loanRepository.SaveChangesAsync();

            var createdDto = _mapper.Map<LoanApplicationDto>(application);
            return CreatedAtRoute("GetApplication", new { id = application.Id }, createdDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Applicant")]
        public async Task<ActionResult> UpdateApplication(int id, [FromBody] LoanApplicationForUpdateDto applicationDto)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(id);
            if (application == null) return NotFound();

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null || application.ApplicantId != int.Parse(userIdStr)) return Forbid();

            if (application.Status != LoanStatus.Draft && application.Status != LoanStatus.Returned)
            {
                return BadRequest("Can only edit applications in Draft or Returned status.");
            }

            _mapper.Map(applicationDto, application);
            
            await _loanRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/submit")]
        [Authorize(Roles = "Applicant")]
        public async Task<ActionResult> SubmitApplication(int id)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(id);
            if (application == null) return NotFound();

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null || application.ApplicantId != int.Parse(userIdStr)) return Forbid();

            if (application.Status != LoanStatus.Draft && application.Status != LoanStatus.Returned)
            {
                return BadRequest("Can only submit applications in Draft or Returned status.");
            }

            if (application.TermInMonths > 0)
            {
                decimal estimatedMonthlyPayment = application.Amount / application.TermInMonths;
                if (estimatedMonthlyPayment > application.MonthlyIncome)
                {
                    return BadRequest($"Submission rejected: Your monthly income ({application.MonthlyIncome:C}) is insufficient for the estimated monthly payment of {estimatedMonthlyPayment:C}.");
                }
            }

            application.Status = LoanStatus.Submitted;
            await _loanRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "Applicant")]
        public async Task<ActionResult> CancelApplication(int id)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(id);
            if (application == null) return NotFound();

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null || application.ApplicantId != int.Parse(userIdStr)) return Forbid();

            if (application.Status != LoanStatus.Draft && application.Status != LoanStatus.Returned && application.Status != LoanStatus.Submitted)
            {
                return BadRequest("Application cannot be cancelled at this stage.");
            }

            application.Status = LoanStatus.Cancelled;
            await _loanRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/review")]
        [Authorize(Roles = "Reviewer")]
        public async Task<ActionResult> ReviewApplication(int id, [FromBody] ReviewDto reviewDto)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(id);
            if (application == null) return NotFound();

            if (application.Status != LoanStatus.Submitted)
            {
                return BadRequest("Can only review submitted applications.");
            }

            application.Status = reviewDto.Status;
            application.Remarks = reviewDto.Remarks;

            await _loanRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Approver")]
        public async Task<ActionResult> ApproveApplication(int id, [FromBody] ApproveDto approveDto)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(id);
            if (application == null) return NotFound();

            if (application.Status != LoanStatus.Reviewed)
            {
                return BadRequest("Can only process applications that have been reviewed.");
            }

            application.Status = approveDto.Status;
            application.Remarks = approveDto.Remarks;

            await _loanRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/release")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ReleaseFunds(int id)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(id);
            if (application == null) return NotFound();

            if (application.Status != LoanStatus.Approved)
            {
                return BadRequest("Can only release funds for approved applications.");
            }

            var treasury = await _loanRepository.GetTreasuryAsync();
            if (treasury == null || treasury.Balance < application.Amount)
            {
                return BadRequest("Insufficient treasury funds to release this loan.");
            }

            treasury.Balance -= application.Amount;
            
            _loanRepository.AddTreasuryTransaction(new TreasuryTransaction
            {
                Amount = -application.Amount,
                TransactionDate = DateTime.UtcNow,
                Type = "FundRelease",
                ReferenceId = application.Id
            });

            decimal totalAmountOwed = application.Amount + (application.Amount * application.InterestRate);
            decimal monthlyAmount = totalAmountOwed / application.TermInMonths;
            for (int i = 1; i <= application.TermInMonths; i++)
            {
                _loanRepository.AddPaymentSchedule(new PaymentSchedule
                {
                    LoanApplicationId = application.Id,
                    DueDate = DateTime.UtcNow.AddMonths(i),
                    AmountDue = monthlyAmount,
                    AmountPaid = 0,
                    Status = PaymentStatus.Pending
                });
            }

            application.Status = LoanStatus.Released;
            await _loanRepository.SaveChangesAsync();
            return NoContent();
        }
    }
}
