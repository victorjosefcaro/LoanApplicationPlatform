namespace LoanApplicationPlatform.API.Models
{
    public class LoanApplicationStatusHistoryDto
    {
        public int Id { get; set; }
        public int LoanApplicationId { get; set; }
        public string? PreviousStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public int ChangedByUserId { get; set; }
        public string? ChangedByUsername { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
