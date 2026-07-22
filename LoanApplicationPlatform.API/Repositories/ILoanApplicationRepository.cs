using LoanApplicationPlatform.API.Entities;
using LoanApplicationPlatform.API.Helpers;

namespace LoanApplicationPlatform.API.Repositories
{
    public interface ILoanApplicationRepository
    {
        Task<PagedList<LoanApplication>> GetLoanApplicationsAsync(ResourceParameters parameters, int? applicantId = null, string? status = null);
        Task<LoanApplication?> GetLoanApplicationAsync(int loanApplicationId);
        void AddLoanApplication(LoanApplication loanApplication);

        Task<IEnumerable<PaymentSchedule>> GetPaymentSchedulesAsync(int loanApplicationId);
        void AddPaymentSchedule(PaymentSchedule paymentSchedule);

        Task<bool> SaveChangesAsync();
    }
}
