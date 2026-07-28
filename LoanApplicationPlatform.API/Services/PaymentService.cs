using AutoMapper;
using LoanApplicationPlatform.API.Constants;
using LoanApplicationPlatform.API.Entities;
using LoanApplicationPlatform.API.Models;
using LoanApplicationPlatform.API.Repositories;

namespace LoanApplicationPlatform.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ILoanApplicationRepository _loanRepository;
        private readonly ITreasuryRepository _treasuryRepository;
        private readonly IMapper _mapper;

        public PaymentService(
            ILoanApplicationRepository loanRepository,
            ITreasuryRepository treasuryRepository,
            IMapper mapper)
        {
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            _treasuryRepository = treasuryRepository ?? throw new ArgumentNullException(nameof(treasuryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<(IEnumerable<PaymentScheduleDto>? Schedules, string? ErrorMessage, bool NotFound, bool Forbid)> GetPaymentSchedulesAsync(int loanApplicationId, int userId, string role)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(loanApplicationId);
            if (application == null) return (null, "Application not found.", true, false);

            if (role == "Applicant" && application.ApplicantId != userId)
            {
                return (null, "Access denied.", false, true);
            }

            var schedules = await _loanRepository.GetPaymentSchedulesAsync(loanApplicationId);
            return (_mapper.Map<IEnumerable<PaymentScheduleDto>>(schedules), null, false, false);
        }

        public async Task<(bool Success, string? ErrorMessage, bool NotFound, bool Forbid)> SubmitPaymentAsync(int loanApplicationId, int scheduleId, int userId, decimal? amount = null)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(loanApplicationId);
            if (application == null) return (false, "Application not found.", true, false);

            if (application.ApplicantId != userId) return (false, "Access denied.", false, true);

            var schedules = await _loanRepository.GetPaymentSchedulesAsync(loanApplicationId);
            var scheduleToPay = schedules.FirstOrDefault(s => s.Id == scheduleId);

            if (scheduleToPay == null) return (false, "Payment schedule not found.", true, false);

            if (scheduleToPay.Status == PaymentStatus.Paid) return (false, "This schedule is already paid.", false, false);

            scheduleToPay.SubmittedAmount = amount ?? (scheduleToPay.AmountDue - scheduleToPay.AmountPaid);
            scheduleToPay.Status = PaymentStatus.PaymentSubmitted;

            await _loanRepository.SaveChangesAsync();
            return (true, null, false, false);
        }

        public async Task<(bool Success, string? ErrorMessage, bool NotFound)> PostPaymentAsync(int loanApplicationId, int scheduleId, PaymentDto paymentDto)
        {
            var schedules = await _loanRepository.GetPaymentSchedulesAsync(loanApplicationId);
            var scheduleToPost = schedules.FirstOrDefault(s => s.Id == scheduleId);

            if (scheduleToPost == null) return (false, "Payment schedule not found.", true);

            if (scheduleToPost.Status != PaymentStatus.PaymentSubmitted && scheduleToPost.Status != PaymentStatus.PartiallyPaid)
                return (false, "Schedule must be in Submitted or Partially Paid status to post.", false);

            if (paymentDto.Amount > scheduleToPost.AmountDue - scheduleToPost.AmountPaid)
            {
                return (false, "Payment amount exceeds the remaining balance for this schedule.", false);
            }

            scheduleToPost.AmountPaid += paymentDto.Amount;

            if (scheduleToPost.AmountPaid >= scheduleToPost.AmountDue)
            {
                scheduleToPost.Status = PaymentStatus.Paid;
            }
            else
            {
                scheduleToPost.Status = PaymentStatus.PartiallyPaid;
            }

            // Update Treasury
            var treasury = await _treasuryRepository.GetTreasuryAsync();
            if (treasury != null)
            {
                treasury.Balance += paymentDto.Amount;

                _treasuryRepository.AddTreasuryTransaction(new TreasuryTransaction
                {
                    Amount = paymentDto.Amount,
                    TransactionDate = DateTime.UtcNow,
                    Type = "PaymentReceived",
                    ReferenceId = loanApplicationId
                });
            }

            // Loan Closure check
            var allSchedules = await _loanRepository.GetPaymentSchedulesAsync(loanApplicationId);
            if (allSchedules.All(s => s.Status == PaymentStatus.Paid))
            {
                var application = await _loanRepository.GetLoanApplicationAsync(loanApplicationId);
                if (application != null)
                {
                    application.Status = LoanStatus.Completed;
                }
            }

            await _loanRepository.SaveChangesAsync();
            await _treasuryRepository.SaveChangesAsync();
            return (true, null, false);
        }
    }
}
