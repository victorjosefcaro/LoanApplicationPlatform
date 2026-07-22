using AutoMapper;
using LoanApplicationPlatform.API.Constants;
using LoanApplicationPlatform.API.Entities;
using LoanApplicationPlatform.API.Helpers;
using LoanApplicationPlatform.API.Models;
using LoanApplicationPlatform.API.Repositories;

namespace LoanApplicationPlatform.API.Services
{
    public class LoanApplicationService : ILoanApplicationService
    {
        private readonly ILoanApplicationRepository _loanRepository;
        private readonly ITreasuryRepository _treasuryRepository;
        private readonly IMapper _mapper;

        public LoanApplicationService(
            ILoanApplicationRepository loanRepository,
            ITreasuryRepository treasuryRepository,
            IMapper mapper)
        {
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            _treasuryRepository = treasuryRepository ?? throw new ArgumentNullException(nameof(treasuryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PagedList<LoanApplicationDto>> GetApplicationsAsync(int userId, string role, ResourceParameters parameters)
        {
            PagedList<LoanApplication> applications;

            if (role == "Applicant")
            {
                applications = await _loanRepository.GetLoanApplicationsAsync(parameters, applicantId: userId);
            }
            else
            {
                applications = await _loanRepository.GetLoanApplicationsAsync(parameters);
            }

            var dtos = _mapper.Map<IEnumerable<LoanApplicationDto>>(applications);
            return new PagedList<LoanApplicationDto>(dtos.ToList(), applications.TotalCount, applications.CurrentPage, applications.PageSize);
        }

        public async Task<(LoanApplicationDto? Dto, string? ErrorMessage, bool NotFound, bool Forbid)> GetApplicationAsync(int id, int userId, string role)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(id);
            if (application == null) return (null, "Application not found.", true, false);

            if (role == "Applicant" && application.ApplicantId != userId)
            {
                return (null, "Access denied.", false, true);
            }

            return (_mapper.Map<LoanApplicationDto>(application), null, false, false);
        }

        public async Task<(LoanApplicationDto? Dto, string? ErrorMessage)> CreateApplicationAsync(int userId, LoanApplicationForCreationDto dto)
        {
            if (dto.TermInMonths > 0)
            {
                decimal estimatedMonthlyPayment = dto.Amount / dto.TermInMonths;
                if (estimatedMonthlyPayment > dto.MonthlyIncome)
                {
                    return (null, $"Submission rejected: Your monthly income ({dto.MonthlyIncome:C}) is insufficient for the estimated monthly payment of {estimatedMonthlyPayment:C}.");
                }
            }

            var application = _mapper.Map<LoanApplication>(dto);
            application.ApplicantId = userId;
            application.Status = LoanStatus.Submitted;
            application.InterestRate = 0.05m;
            application.CreatedAt = DateTime.UtcNow;

            _loanRepository.AddLoanApplication(application);
            await _loanRepository.SaveChangesAsync();

            return (_mapper.Map<LoanApplicationDto>(application), null);
        }

        public async Task<(bool Success, string? ErrorMessage, bool NotFound, bool Forbid)> UpdateApplicationAsync(int id, int userId, LoanApplicationForUpdateDto dto)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(id);
            if (application == null) return (false, "Application not found.", true, false);

            if (application.ApplicantId != userId) return (false, "Access denied.", false, true);

            if (application.Status != LoanStatus.Draft && application.Status != LoanStatus.Returned)
            {
                return (false, "Can only edit applications in Draft or Returned status.", false, false);
            }

            _mapper.Map(dto, application);
            await _loanRepository.SaveChangesAsync();

            return (true, null, false, false);
        }

        public async Task<(bool Success, string? ErrorMessage, bool NotFound, bool Forbid)> SubmitApplicationAsync(int id, int userId)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(id);
            if (application == null) return (false, "Application not found.", true, false);

            if (application.ApplicantId != userId) return (false, "Access denied.", false, true);

            if (application.Status != LoanStatus.Draft && application.Status != LoanStatus.Returned)
            {
                return (false, "Can only submit applications in Draft or Returned status.", false, false);
            }

            if (application.TermInMonths > 0)
            {
                decimal estimatedMonthlyPayment = application.Amount / application.TermInMonths;
                if (estimatedMonthlyPayment > application.MonthlyIncome)
                {
                    return (false, $"Submission rejected: Your monthly income ({application.MonthlyIncome:C}) is insufficient for the estimated monthly payment of {estimatedMonthlyPayment:C}.", false, false);
                }
            }

            application.Status = LoanStatus.Submitted;
            await _loanRepository.SaveChangesAsync();

            return (true, null, false, false);
        }

        public async Task<(bool Success, string? ErrorMessage, bool NotFound, bool Forbid)> CancelApplicationAsync(int id, int userId)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(id);
            if (application == null) return (false, "Application not found.", true, false);

            if (application.ApplicantId != userId) return (false, "Access denied.", false, true);

            if (application.Status != LoanStatus.Draft && application.Status != LoanStatus.Returned && application.Status != LoanStatus.Submitted)
            {
                return (false, "Application cannot be cancelled at this stage.", false, false);
            }

            application.Status = LoanStatus.Cancelled;
            await _loanRepository.SaveChangesAsync();

            return (true, null, false, false);
        }

        public async Task<(bool Success, string? ErrorMessage, bool NotFound)> ReviewApplicationAsync(int id, ReviewDto reviewDto)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(id);
            if (application == null) return (false, "Application not found.", true);

            if (application.Status != LoanStatus.Submitted)
            {
                return (false, "Can only review submitted applications.", false);
            }

            application.Status = Enum.Parse<LoanStatus>(reviewDto.Status);
            application.Remarks = reviewDto.Remarks;

            await _loanRepository.SaveChangesAsync();
            return (true, null, false);
        }

        public async Task<(bool Success, string? ErrorMessage, bool NotFound)> ApproveApplicationAsync(int id, ApproveDto approveDto)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(id);
            if (application == null) return (false, "Application not found.", true);

            if (application.Status != LoanStatus.Reviewed)
            {
                return (false, "Can only process applications that have been reviewed.", false);
            }

            application.Status = Enum.Parse<LoanStatus>(approveDto.Status);
            application.Remarks = approveDto.Remarks;

            await _loanRepository.SaveChangesAsync();
            return (true, null, false);
        }

        public async Task<(bool Success, string? ErrorMessage, bool NotFound)> ReleaseFundsAsync(int id)
        {
            var application = await _loanRepository.GetLoanApplicationAsync(id);
            if (application == null) return (false, "Application not found.", true);

            if (application.Status != LoanStatus.Approved)
            {
                return (false, "Can only release funds for approved applications.", false);
            }

            var treasury = await _treasuryRepository.GetTreasuryAsync();
            if (treasury == null || treasury.Balance < application.Amount)
            {
                return (false, "Insufficient treasury funds to release this loan.", false);
            }

            treasury.Balance -= application.Amount;

            _treasuryRepository.AddTreasuryTransaction(new TreasuryTransaction
            {
                Amount = -application.Amount,
                TransactionDate = DateTime.UtcNow,
                Type = "FundRelease",
                ReferenceId = application.Id
            });

            decimal totalAmountOwed = application.Amount + (application.Amount * application.InterestRate);
            decimal monthlyAmount = totalAmountOwed / application.TermInMonths;

            for (int i = 1; i <= application.TermInMonths; i++)
            {
                _loanRepository.AddPaymentSchedule(new PaymentSchedule
                {
                    LoanApplicationId = application.Id,
                    DueDate = DateTime.UtcNow.AddMonths(i),
                    AmountDue = monthlyAmount,
                    AmountPaid = 0,
                    Status = PaymentStatus.Pending
                });
            }

            application.Status = LoanStatus.Released;
            await _loanRepository.SaveChangesAsync();
            await _treasuryRepository.SaveChangesAsync();

            return (true, null, false);
        }
    }
}
