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
            var collection = _context.TreasuryTransactions as IQueryable<TreasuryTransaction>;

            if (!string.IsNullOrWhiteSpace(parameters.SearchQuery))
            {
                var search = parameters.SearchQuery.Trim();
                var cleanSearch = search.TrimStart('#');
                bool isInt = int.TryParse(cleanSearch, out int searchId);

                collection = collection.Where(t =>
                    t.Type.Contains(search) ||
                    (isInt && (t.Id == searchId || t.ReferenceId == searchId)));
            }

            var orderedCollection = collection.OrderByDescending(t => t.TransactionDate);
            return await PagedList<TreasuryTransaction>.CreateAsync(orderedCollection, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }
    }
}
