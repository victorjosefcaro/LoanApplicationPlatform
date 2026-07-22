using LoanApplicationPlatform.ConsoleApp.Services;
using LoanApplicationPlatform.ConsoleApp.Menus;
using System;
using System.Threading.Tasks;

Console.OutputEncoding = System.Text.Encoding.UTF8;

const string ApiBaseUrl = "https://localhost:7103";
using var apiClient = new LoanApiClient(ApiBaseUrl);
string? currentRole = null;

void Logout()
{
    apiClient.ClearToken();
    currentRole = null;
    ConsoleHelper.PrintSuccess("Logged out successfully.");
}

while (true)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("=====================================");
    Console.WriteLine("     LOAN APPLICATION PLATFORM       ");
    Console.WriteLine("=====================================");
    Console.ResetColor();

    if (apiClient.JwtToken != null)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(apiClient.JwtToken);
        var tenantId = token.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[ Status: Logged In | Role: {currentRole} | Tenant ID: {tenantId} ]");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[ Status: NOT Logged In ]");
        Console.ResetColor();
    }

    try
    {
    if (apiClient.JwtToken == null)
    {
        Console.WriteLine("\n1. Login");
        Console.WriteLine("2. Register (Applicant)");
        Console.WriteLine("3. Exit");
        Console.Write("\nSelect an option: ");
        
        var choice = Console.ReadLine();
        switch (choice)
        {
            case "1": 
                currentRole = await AuthenticationMenu.LoginAsync(apiClient); 
                break;
            case "2": 
                await AuthenticationMenu.RegisterAsync(apiClient); 
                break;
            case "3": 
                return;
            default: 
                ConsoleHelper.PrintError("Invalid option."); 
                break;
        }
    }
    else
    {
        if (currentRole == "Applicant") await ApplicantMenu.RunAsync(apiClient, Logout);
        else if (currentRole == "Reviewer") await ReviewerMenu.RunAsync(apiClient, Logout);
        else if (currentRole == "Approver") await ApproverMenu.RunAsync(apiClient, Logout);
        else if (currentRole == "Admin") await AdminMenu.RunAsync(apiClient, Logout);
        else 
        {
            ConsoleHelper.PrintError("Unknown role. Logging out...");
            Logout();
        }
    }
    }
    catch (HttpRequestException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nConnection error: {ex.Message}");
        Console.WriteLine("Make sure the API server is running.");
        Console.ResetColor();
        ConsoleHelper.WaitForKey();
    }
}
