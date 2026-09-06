using System.ComponentModel.DataAnnotations;

namespace LoanApplicationPlatform.API.Helpers
{
    public class UserResourceParameters
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

        /// <summary>Optional case-insensitive username filter.</summary>
        public string? Search { get; set; }
    }
}
