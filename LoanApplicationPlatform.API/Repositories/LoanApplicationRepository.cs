using LoanApplicationPlatform.API.DbContexts;
using LoanApplicationPlatform.API.Entities;
using LoanApplicationPlatform.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LoanApplicationPlatform.API.Repositories
{
    public class LoanApplicationRepository : ILoanApplicationRepository
    {
        private readonly LoanApplicationPlatformContext _context;

        public LoanApplicationRepository(LoanApplicationPlatformContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedList<LoanApplication>> GetLoanApplicationsAsync(ResourceParameters parameters, int? applicantId = null, string? status = null)
        {
            var collection = _context.LoanApplications as IQueryable<LoanApplication>;

            if (applicantId.HasValue)
            {
                collection = collection.Where(a => a.ApplicantId == applicantId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Constants.LoanStatus>(status.Trim(), out var parsedStatus))
            {
                collection = collection.Where(a => a.Status == parsedStatus);
            }

            var orderedCollection = collection.OrderByDescending(a => a.CreatedAt);
            return await PagedList<LoanApplication>.CreateAsync(orderedCollection, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<LoanApplication?> GetLoanApplicationAsync(int loanApplicationId)
        {
            return await _context.LoanApplications
                .FirstOrDefaultAsync(a => a.Id == loanApplicationId);
        }

        public void AddLoanApplication(LoanApplication loanApplication)
        {
            _context.LoanApplications.Add(loanApplication);
        }

        public async Task<IEnumerable<PaymentSchedule>> GetPaymentSchedulesAsync(int loanApplicationId)
        {
            return await _context.PaymentSchedules
                .Where(p => p.LoanApplicationId == loanApplicationId)
                .OrderBy(p => p.DueDate)
                .ToListAsync();
        }

        public void AddPaymentSchedule(PaymentSchedule paymentSchedule)
        {
            _context.PaymentSchedules.Add(paymentSchedule);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }
    }
}
