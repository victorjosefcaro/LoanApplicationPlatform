using System.ComponentModel.DataAnnotations;

namespace LoanApplicationPlatform.API.Helpers
{
    public class ResourceParameters
    {
        const int maxPageSize = 50;

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;
        [Range(1, maxPageSize)]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        [RegularExpression("^(Submitted|Returned|Reviewed|Approved|Rejected|Released|Cancelled|Completed)$",
            ErrorMessage = "Status must be a valid loan status.")]
        public string? Status { get; set; }
    }
}
