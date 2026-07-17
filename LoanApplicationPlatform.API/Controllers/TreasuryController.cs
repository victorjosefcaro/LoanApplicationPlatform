using LoanApplicationPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplicationPlatform.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Approver")]
    [Route("api/treasury")]
    public class TreasuryController : ControllerBase
    {
        private readonly ILoanRepository _loanRepository;

        public TreasuryController(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        }

        [HttpGet("balance")]
        public async Task<ActionResult> GetBalance()
        {
            var treasury = await _loanRepository.GetTreasuryAsync();
            if (treasury == null) return NotFound("Treasury record not found.");
            
            return Ok(new { balance = treasury.Balance });
        }
    }
}
