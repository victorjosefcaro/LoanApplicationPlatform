using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LoanApplicationPlatform.ConsoleApp.Models;

namespace LoanApplicationPlatform.ConsoleApp.Services
{
    public class LoanApiClient
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
            var response = await _httpClient.PostAsJsonAsync("/api/authentication/authenticate", new { Username = username, Password = password });
            if (response.IsSuccessStatusCode)
            {
                SetToken(await response.Content.ReadAsStringAsync());
                return (true, null);
            }
            return (false, $"Status: {response.StatusCode}");
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> RegisterAsync(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/authentication/register", new { Username = username, Password = password });
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await response.Content.ReadAsStringAsync());
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> AdminRegisterUserAsync(string username, string password, string role)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/authentication/admin/register", new { Username = username, Password = password, Role = role });
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await response.Content.ReadAsStringAsync());
        }

        public async Task<IEnumerable<LoanApplicationDto>?> GetApplicationsAsync()
        {
            var response = await _httpClient.GetAsync("/api/loanapplications");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<LoanApplicationDto>>(_jsonOptions);
            }
            return null;
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

        public async Task<bool> SubmitApplicationAsync(int id)
        {
            var response = await _httpClient.PatchAsync($"/api/loanapplications/{id}/submit", null);
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

        public async Task<bool> SubmitPaymentAsync(int loanApplicationId, int scheduleId)
        {
            var response = await _httpClient.PostAsync($"/api/loanapplications/{loanApplicationId}/payments/{scheduleId}/submit", null);
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
            var response = await _httpClient.PostAsJsonAsync("/api/treasury/deposit", new { Amount = amount });
            return response.IsSuccessStatusCode;
        }
    }
}
