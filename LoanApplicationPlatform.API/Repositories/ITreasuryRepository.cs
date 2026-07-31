using LoanApplicationPlatform.API.Entities;
using LoanApplicationPlatform.API.Helpers;

namespace LoanApplicationPlatform.API.Repositories
{
    public interface ITreasuryRepository
    {
        Task<Treasury?> GetTreasuryAsync();
        void AddTreasuryTransaction(TreasuryTransaction transaction);
        Task<PagedList<TreasuryTransaction>> GetTreasuryTransactionsAsync(ResourceParameters parameters);
        Task<bool> SaveChangesAsync();
    }
}
