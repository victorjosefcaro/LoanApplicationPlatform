using LoanApplicationPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplicationPlatform.API.Controllers
{
    [ApiController]
    [Route("api/treasury")]
    public class TreasuryController : ControllerBase
    {
        private readonly ILoanRepository _loanRepository;

        public TreasuryController(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        }

        [HttpGet("balance")]
        [Authorize(Roles = "Admin,Approver")]
        public async Task<ActionResult> GetBalance()
        {
            var treasury = await _loanRepository.GetTreasuryAsync();
            if (treasury == null) return NotFound("Treasury record not found.");
            
            return Ok(new { balance = treasury.Balance });
        }

        [HttpPost("deposit")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DepositFunds([FromBody] LoanApplicationPlatform.API.Models.PaymentDto depositDto)
        {
            if (depositDto.Amount <= 0) return BadRequest("Deposit amount must be positive.");

            var treasury = await _loanRepository.GetTreasuryAsync();
            if (treasury == null) return NotFound("Treasury record not found.");
            
            treasury.Balance += depositDto.Amount;
            await _loanRepository.SaveChangesAsync();

            return Ok(new { balance = treasury.Balance });
        }
    }
}
