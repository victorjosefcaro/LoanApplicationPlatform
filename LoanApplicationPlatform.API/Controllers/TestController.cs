using LoanApplicationPlatform.API.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanApplicationPlatform.API.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly LoanApplicationPlatformContext _context;

        public TestController(LoanApplicationPlatformContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet("treasury")]
        public async Task<IActionResult> GetTreasuryBalance()
        {
            var treasury = await _context.Treasury.FirstOrDefaultAsync();
            if (treasury == null)
            {
                return NotFound("Treasury not found.");
            }

            return Ok(new { Balance = treasury.Balance });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }
    }
}
