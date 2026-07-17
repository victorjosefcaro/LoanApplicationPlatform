namespace LoanApplicationPlatform.API.Models
{
    public class PaymentScheduleDto
    {
        public int Id { get; set; }
        public int LoanApplicationId { get; set; }
        public DateTime DueDate { get; set; }
        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
