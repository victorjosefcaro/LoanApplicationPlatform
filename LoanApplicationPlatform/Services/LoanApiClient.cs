using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LoanApplicationPlatform.ConsoleApp.Models;

namespace LoanApplicationPlatform.ConsoleApp.Services
{
    public class LoanApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public string? JwtToken { get; private set; }

        public LoanApiClient(string baseAddress)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public void SetToken(string token)
        {
            JwtToken = token.Trim('"');
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);
        }

        public void ClearToken()
        {
            JwtToken = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> LoginAsync(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/authentication/login", new { Username = username, Password = password });
            if (response.IsSuccessStatusCode)
            {
                SetToken(await response.Content.ReadAsStringAsync());
                return (true, null);
            }
            return (false, $"Status: {response.StatusCode}");
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> RegisterAsync(string username, string password, int? tenantId = 1)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/authentication/register", new { Username = username, Password = password, TenantId = tenantId });
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await response.Content.ReadAsStringAsync());
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> AdminRegisterUserAsync(string username, string password, string role, int? tenantId = 1)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/authentication/admin/register", new { Username = username, Password = password, Role = role, TenantId = tenantId });
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await response.Content.ReadAsStringAsync());
        }

        public async Task<PagedResponse<ApplicationDto>?> GetApplicationsAsync(int pageNumber = 1, int pageSize = 10, string? status = null)
        {
            try
            {
                var url = $"api/loanapplications?pageNumber={pageNumber}&pageSize={pageSize}";
                if (!string.IsNullOrWhiteSpace(status))
                {
                    url += $"&status={Uri.EscapeDataString(status)}";
                }
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<IEnumerable<ApplicationDto>>();
                    PaginationMetadata? metadata = null;
                    if (response.Headers.TryGetValues("X-Pagination", out var values))
                    {
                        var json = values.FirstOrDefault();
                        if (json != null)
                        {
                            metadata = System.Text.Json.JsonSerializer.Deserialize<PaginationMetadata>(json);
                        }
                    }
                    return new PagedResponse<ApplicationDto>(data ?? new List<ApplicationDto>(), metadata);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CreateApplicationAsync(object applicationData)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/loanapplications", applicationData);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateApplicationAsync(int id, object applicationData)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/loanapplications/{id}", applicationData);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CancelApplicationAsync(int id)
        {
            var response = await _httpClient.PatchAsync($"/api/loanapplications/{id}/cancel", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ReviewApplicationAsync(int id, string status, string? remarks)
        {
            var response = await _httpClient.PatchAsJsonAsync($"/api/loanapplications/{id}/review", new { Status = status, Remarks = remarks });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ApproveApplicationAsync(int id, string status, string? remarks)
        {
            var response = await _httpClient.PatchAsJsonAsync($"/api/loanapplications/{id}/approve", new { Status = status, Remarks = remarks });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ReleaseFundsAsync(int id)
        {
            var response = await _httpClient.PostAsync($"/api/loanapplications/{id}/release", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<PaymentScheduleDto>?> GetPaymentSchedulesAsync(int loanApplicationId)
        {
            var response = await _httpClient.GetAsync($"/api/loanapplications/{loanApplicationId}/payments");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<PaymentScheduleDto>>(_jsonOptions);
            }
            return null;
        }

        public async Task<bool> SubmitPaymentAsync(int loanApplicationId, int scheduleId, decimal? amount = null)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/loanapplications/{loanApplicationId}/payments/{scheduleId}/submit", new { Amount = amount });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PostPaymentAsync(int loanApplicationId, int scheduleId, decimal amount)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/loanapplications/{loanApplicationId}/payments/{scheduleId}/post", new { Amount = amount });
            return response.IsSuccessStatusCode;
        }

        public async Task<decimal?> GetTreasuryBalanceAsync()
        {
            var response = await _httpClient.GetAsync("/api/treasury/balance");
            if (response.IsSuccessStatusCode)
            {
                var dict = await response.Content.ReadFromJsonAsync<Dictionary<string, decimal>>();
                return dict?["balance"];
            }
            return null;
        }

        public async Task<bool> DepositToTreasuryAsync(decimal amount)
        {
            var response = await _httpClient.PostAsJsonAsync("api/treasury/deposit", new { amount });
            return response.IsSuccessStatusCode;
        }

        public async Task<PagedResponse<TreasuryTransactionDto>?> GetTreasuryTransactionsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/treasury/transactions?pageNumber={pageNumber}&pageSize={pageSize}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<IEnumerable<TreasuryTransactionDto>>();
                    PaginationMetadata? metadata = null;
                    if (response.Headers.TryGetValues("X-Pagination", out var values))
                    {
                        var json = values.FirstOrDefault();
                        if (json != null)
                        {
                            metadata = System.Text.Json.JsonSerializer.Deserialize<PaginationMetadata>(json);
                        }
                    }
                    return new PagedResponse<TreasuryTransactionDto>(data ?? new List<TreasuryTransactionDto>(), metadata);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }

    public record TreasuryTransactionDto(int Id, DateTime TransactionDate, decimal Amount, string Type, int? ReferenceId);
    public record ApplicationDto(int Id, decimal Amount, int TermInMonths, string Status, string Remarks, DateTime CreatedAt);
    public record PaymentScheduleDto(int Id, DateTime DueDate, decimal AmountDue, decimal AmountPaid, decimal? SubmittedAmount, string Status);
    
    public record PaginationMetadata(int totalCount, int pageSize, int currentPage, int totalPages, bool hasPrevious, bool hasNext);
    public record PagedResponse<T>(IEnumerable<T> Items, PaginationMetadata? Metadata);
}
