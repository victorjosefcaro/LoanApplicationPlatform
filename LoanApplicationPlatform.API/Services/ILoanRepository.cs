using LoanApplicationPlatform.API.Entities;
using LoanApplicationPlatform.API.Helpers;

namespace LoanApplicationPlatform.API.Services
{
    public interface ILoanRepository
    {
        Task<PagedList<LoanApplication>> GetLoanApplicationsAsync(ResourceParameters parameters, int? applicantId = null, string? status = null);
        Task<LoanApplication?> GetLoanApplicationAsync(int loanApplicationId);
        void AddLoanApplication(LoanApplication loanApplication);
        
        Task<IEnumerable<PaymentSchedule>> GetPaymentSchedulesAsync(int loanApplicationId);
        void AddPaymentSchedule(PaymentSchedule paymentSchedule);
        
        Task<Treasury?> GetTreasuryAsync();
        void AddTreasuryTransaction(TreasuryTransaction transaction);
        Task<PagedList<TreasuryTransaction>> GetTreasuryTransactionsAsync(ResourceParameters parameters);
        
        Task<bool> SaveChangesAsync();
    }
}
