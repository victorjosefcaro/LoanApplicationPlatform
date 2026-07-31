using System;
using System.Threading.Tasks;
using LoanApplicationPlatform.ConsoleApp.Services;

namespace LoanApplicationPlatform.ConsoleApp.Menus
{
    public static class AuthenticationMenu
    {
        public static async Task<string?> LoginAsync(LoanApiClient apiClient)
        {
            ConsoleHelper.PrintHeader("Login");
            Console.Write("Username: ");
            var username = Console.ReadLine() ?? "";
            Console.Write("Password: ");
            var password = Console.ReadLine() ?? "";

            var (success, error) = await apiClient.LoginAsync(username, password);
            if (success)
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(apiClient.JwtToken);
                var currentRole = token.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
                var tenantId = token.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
                ConsoleHelper.PrintSuccess($"Login successful! (Tenant ID: {tenantId})");
                return currentRole;
            }
            else
            {
                ConsoleHelper.PrintError($"Login failed: {error}");
                return null;
            }
        }

        public static async Task RegisterAsync(LoanApiClient apiClient)
        {
            ConsoleHelper.PrintHeader("Register Applicant");
            Console.Write("New Username: ");
            var username = Console.ReadLine() ?? "";
            Console.Write("New Password: ");
            var password = Console.ReadLine() ?? "";

            Console.WriteLine("\nSelect Tenant:");
            Console.WriteLine("1. Default Lending Co (Tenant ID: 1)");
            Console.WriteLine("2. Acme Finance (Tenant ID: 2)");
            Console.Write("Choice (1 or 2, default 1): ");
            var tenantChoice = Console.ReadLine()?.Trim();
            int tenantId = tenantChoice == "2" ? 2 : 1;

            var (success, error) = await apiClient.RegisterAsync(username, password, tenantId);
            if (success) ConsoleHelper.PrintSuccess($"Registration successful for Tenant {tenantId}! You can now login.");
            else ConsoleHelper.PrintError($"Registration failed: {error}");
        }
    }
}
