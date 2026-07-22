using LoanApplicationPlatform.API.Models;

namespace LoanApplicationPlatform.API.Services
{
    public interface IPaymentService
    {
        Task<(IEnumerable<PaymentScheduleDto>? Schedules, string? ErrorMessage, bool NotFound, bool Forbid)> GetPaymentSchedulesAsync(int loanApplicationId, int userId, string role);
        Task<(bool Success, string? ErrorMessage, bool NotFound, bool Forbid)> SubmitPaymentAsync(int loanApplicationId, int scheduleId, int userId);
        Task<(bool Success, string? ErrorMessage, bool NotFound)> PostPaymentAsync(int loanApplicationId, int scheduleId, PaymentDto paymentDto);
    }
}
