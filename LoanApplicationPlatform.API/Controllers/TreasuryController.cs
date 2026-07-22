using LoanApplicationPlatform.API.Entities;
using LoanApplicationPlatform.API.Helpers;
using LoanApplicationPlatform.API.Models;
using LoanApplicationPlatform.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplicationPlatform.API.Controllers
{
    [ApiController]
    [Route("api/treasury")]
    public class TreasuryController : ControllerBase
    {
        private readonly ITreasuryRepository _treasuryRepository;

        public TreasuryController(ITreasuryRepository treasuryRepository)
        {
            _treasuryRepository = treasuryRepository ?? throw new ArgumentNullException(nameof(treasuryRepository));
        }

        [HttpGet("balance")]
        [Authorize(Roles = "Admin,Approver")]
        public async Task<ActionResult> GetBalance()
        {
            var treasury = await _treasuryRepository.GetTreasuryAsync();
            if (treasury == null) return NotFound("Treasury record not found.");

            return Ok(new { balance = treasury.Balance });
        }

        [HttpPost("deposit")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DepositFunds([FromBody] DepositDto depositDto)
        {
            if (depositDto.Amount <= 0) return BadRequest("Deposit amount must be positive.");

            var treasury = await _treasuryRepository.GetTreasuryAsync();
            if (treasury == null) return NotFound("Treasury record not found.");

            treasury.Balance += depositDto.Amount;

            _treasuryRepository.AddTreasuryTransaction(new TreasuryTransaction
            {
                Amount = depositDto.Amount,
                TransactionDate = DateTime.UtcNow,
                Type = "Deposit",
                ReferenceId = null
            });

            await _treasuryRepository.SaveChangesAsync();

            return Ok(new { balance = treasury.Balance });
        }

        [HttpGet("transactions")]
        [Authorize(Roles = "Admin,Approver")]
        public async Task<ActionResult> GetTransactions([FromQuery] ResourceParameters parameters)
        {
            var transactions = await _treasuryRepository.GetTreasuryTransactionsAsync(parameters);

            var paginationMetadata = new
            {
                totalCount = transactions.TotalCount,
                pageSize = transactions.PageSize,
                currentPage = transactions.CurrentPage,
                totalPages = transactions.TotalPages,
                hasPrevious = transactions.HasPrevious,
                hasNext = transactions.HasNext
            };

            Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(paginationMetadata));

            return Ok(transactions);
        }
    }
}
