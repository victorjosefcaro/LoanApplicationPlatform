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
                ConsoleHelper.PrintSuccess("Login successful!");
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

            var (success, error) = await apiClient.RegisterAsync(username, password);
            if (success) ConsoleHelper.PrintSuccess("Registration successful! You can now login.");
            else ConsoleHelper.PrintError($"Registration failed: {error}");
        }
    }
}
