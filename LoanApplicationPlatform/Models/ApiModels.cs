namespace LoanApplicationPlatform.ConsoleApp.Models
{
    public record TreasuryTransactionDto(int Id, DateTime TransactionDate, decimal Amount, string Type, int? ReferenceId);

    public record ApplicationDto(int Id, decimal Amount, int TermInMonths, string Status, string Remarks, DateTime CreatedAt);

    public record PaginationMetadata(int totalCount, int pageSize, int currentPage, int totalPages, bool hasPrevious, bool hasNext);

    public record PagedResponse<T>(IEnumerable<T> Items, PaginationMetadata? Metadata);
}
