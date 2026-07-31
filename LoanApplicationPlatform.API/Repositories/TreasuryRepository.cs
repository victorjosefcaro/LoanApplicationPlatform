using LoanApplicationPlatform.API.DbContexts;
using LoanApplicationPlatform.API.Entities;
using LoanApplicationPlatform.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LoanApplicationPlatform.API.Repositories
{
    public class TreasuryRepository : ITreasuryRepository
    {
        private readonly LoanApplicationPlatformContext _context;

        public TreasuryRepository(LoanApplicationPlatformContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Treasury?> GetTreasuryAsync()
        {
            return await _context.Treasury.FirstOrDefaultAsync();
        }

        public void AddTreasuryTransaction(TreasuryTransaction transaction)
        {
            _context.TreasuryTransactions.Add(transaction);
        }

        public async Task<PagedList<TreasuryTransaction>> GetTreasuryTransactionsAsync(ResourceParameters parameters)
        {
            var collection = _context.TreasuryTransactions.OrderByDescending(t => t.TransactionDate);
            return await PagedList<TreasuryTransaction>.CreateAsync(collection, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }
    }
}
