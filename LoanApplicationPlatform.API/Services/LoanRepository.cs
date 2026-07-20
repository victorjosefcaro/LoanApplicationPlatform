using LoanApplicationPlatform.API.DbContexts;
using LoanApplicationPlatform.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanApplicationPlatform.API.Services
{
    public class LoanRepository : ILoanRepository
    {
        private readonly LoanApplicationPlatformContext _context;

        public LoanRepository(LoanApplicationPlatformContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<LoanApplication>> GetLoanApplicationsAsync(int? applicantId = null, string? status = null)
        {
            var collection = _context.LoanApplications as IQueryable<LoanApplication>;

            if (applicantId.HasValue)
            {
                collection = collection.Where(a => a.ApplicantId == applicantId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanApplicationPlatform.API.Constants.LoanStatus>(status.Trim(), out var parsedStatus))
            {
                collection = collection.Where(a => a.Status == parsedStatus);
            }

            return await collection.OrderByDescending(a => a.CreatedAt).ToListAsync();
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

        public async Task<Treasury?> GetTreasuryAsync()
        {
            return await _context.Treasury.FirstOrDefaultAsync();
        }

        public void AddTreasuryTransaction(TreasuryTransaction transaction)
        {
            _context.TreasuryTransactions.Add(transaction);
        }

        public async Task<IEnumerable<TreasuryTransaction>> GetTreasuryTransactionsAsync()
        {
            return await _context.TreasuryTransactions
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }
    }
}
