using LoanApplicationPlatform.ConsoleApp.Services;
using LoanApplicationPlatform.ConsoleApp.Menus;
using System;
using System.Threading.Tasks;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var apiClient = new LoanApiClient("https://localhost:7103");
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
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[ Status: Logged In | Role: {currentRole} ]");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[ Status: NOT Logged In ]");
        Console.ResetColor();
    }

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
