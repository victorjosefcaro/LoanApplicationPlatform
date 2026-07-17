using LoanApplicationPlatform.API.Entities;

namespace LoanApplicationPlatform.API.Services
{
    public interface ILoanRepository
    {
        Task<IEnumerable<LoanApplication>> GetLoanApplicationsAsync(int? applicantId = null, string? status = null);
        Task<LoanApplication?> GetLoanApplicationAsync(int loanApplicationId);
        void AddLoanApplication(LoanApplication loanApplication);
        
        Task<IEnumerable<PaymentSchedule>> GetPaymentSchedulesAsync(int loanApplicationId);
        void AddPaymentSchedule(PaymentSchedule paymentSchedule);
        
        Task<Treasury?> GetTreasuryAsync();
        void AddTreasuryTransaction(TreasuryTransaction transaction);
        Task<IEnumerable<TreasuryTransaction>> GetTreasuryTransactionsAsync();
        
        Task<bool> SaveChangesAsync();
    }
}
