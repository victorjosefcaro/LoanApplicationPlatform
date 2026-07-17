using LoanApplicationPlatform.ConsoleApp.Services;
using System.IdentityModel.Tokens.Jwt;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var apiClient = new LoanApiClient("https://localhost:7103");
string? currentRole = null;

while (true)
{
    Console.Clear();
    Console.WriteLine("=== Loan Application Platform ===");
    if (apiClient.JwtToken != null)
    {
        Console.WriteLine($"[ Status: Logged In | Role: {currentRole} ]");
    }
    else
    {
        Console.WriteLine("[ Status: NOT Logged In ]");
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
            case "1": await LoginAsync(); break;
            case "2": await RegisterAsync(); break;
            case "3": return;
            default: Console.WriteLine("\nInvalid option."); WaitForKey(); break;
        }
    }
    else
    {
        // Role-based menu delegation
        if (currentRole == "Applicant") await ApplicantMenuAsync();
        else if (currentRole == "Reviewer") await ReviewerMenuAsync();
        else if (currentRole == "Approver") await ApproverMenuAsync();
        else if (currentRole == "Admin") await AdminMenuAsync();
        else {
            Console.WriteLine("Unknown role. Logging out...");
            Logout();
        }
    }
}

async Task LoginAsync()
{
    Console.Write("Username: ");
    var username = Console.ReadLine() ?? "";
    Console.Write("Password: ");
    var password = Console.ReadLine() ?? "";

    var (success, error) = await apiClient.LoginAsync(username, password);
    if (success)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(apiClient.JwtToken);
        currentRole = token.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
        Console.WriteLine("\nLogin successful!");
    }
    else
    {
        Console.WriteLine($"\nLogin failed: {error}");
    }
    WaitForKey();
}

async Task RegisterAsync()
{
    Console.Write("New Username: ");
    var username = Console.ReadLine() ?? "";
    Console.Write("New Password: ");
    var password = Console.ReadLine() ?? "";

    var (success, error) = await apiClient.RegisterAsync(username, password);
    if (success) Console.WriteLine("\nRegistration successful! You can now login.");
    else Console.WriteLine($"\nRegistration failed: {error}");
    WaitForKey();
}

void Logout()
{
    apiClient.ClearToken();
    currentRole = null;
    Console.WriteLine("\nLogged out successfully.");
    WaitForKey();
}

async Task ApplicantMenuAsync()
{
    Console.WriteLine("\n--- Applicant Menu ---");
    Console.WriteLine("1. View My Applications");
    Console.WriteLine("2. Create & Submit Application");
    Console.WriteLine("3. View Payment Schedules");
    Console.WriteLine("4. Make a Payment");
    Console.WriteLine("5. Logout");
    Console.Write("\nSelect an option: ");
    
    var choice = Console.ReadLine();
    switch (choice)
    {
        case "1":
            var apps = await apiClient.GetApplicationsAsync();
            if (apps != null && apps.Any())
            {
                // Applying LINQ: Order applications by creation date descending
                var activeApps = apps.OrderByDescending(a => a.CreatedAt).ToList();
                Console.WriteLine("\nYour Applications:");
                foreach (var a in activeApps)
                {
                    Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Term: {a.TermInMonths} mos, Status: {a.Status}");
                }
            }
            else Console.WriteLine("\nNo applications found.");
            WaitForKey();
            break;
        case "2":
            Console.Write("Applicant Name: ");
            var name = Console.ReadLine();
            Console.Write("Amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out var amt)) break;
            Console.Write("Term (months): ");
            if (!int.TryParse(Console.ReadLine(), out var term)) break;
            Console.Write("Monthly Income: ");
            if (!decimal.TryParse(Console.ReadLine(), out var inc)) break;
            Console.Write("Purpose: ");
            var purpose = Console.ReadLine();
            
            var success = await apiClient.CreateApplicationAsync(new { ApplicantName = name, Amount = amt, TermInMonths = term, MonthlyIncome = inc, Purpose = purpose });
            Console.WriteLine(success ? "\nApplication successfully created and submitted!" : "\nFailed to create application (check income requirements).");
            WaitForKey();
            break;
        case "3":
            Console.Write("Enter Application ID: ");
            if (int.TryParse(Console.ReadLine(), out var pid))
            {
                var schedules = await apiClient.GetPaymentSchedulesAsync(pid);
                if (schedules != null && schedules.Any())
                {
                    // Applying LINQ: Filter to show only pending schedules to the user
                    var pending = schedules.Where(s => s.Status != "Paid").ToList();
                    Console.WriteLine($"\nYou have {pending.Count} pending schedules out of {schedules.Count()} total.");
                    foreach (var s in schedules)
                    {
                        Console.WriteLine($"- SchID: {s.Id}, Due: {s.DueDate:yyyy-MM-dd}, Amount: {s.AmountDue:C}, Paid: {s.AmountPaid:C}, Status: {s.Status}");
                    }
                }
                else Console.WriteLine("\nNo payment schedules found.");
            }
            WaitForKey();
            break;
        case "4":
            Console.Write("Enter Application ID: ");
            if (!int.TryParse(Console.ReadLine(), out var loanId)) break;
            Console.Write("Enter Schedule ID: ");
            if (!int.TryParse(Console.ReadLine(), out var schId)) break;
            Console.Write("Enter Payment Amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out var payAmt)) break;
            
            var paySuccess = await apiClient.SubmitPaymentAsync(loanId, schId, payAmt);
            Console.WriteLine(paySuccess ? "\nPayment successful!" : "\nPayment failed.");
            WaitForKey();
            break;
        case "5": Logout(); break;
    }
}

async Task ReviewerMenuAsync()
{
    Console.WriteLine("\n--- Reviewer Menu ---");
    Console.WriteLine("1. View Submitted Applications");
    Console.WriteLine("2. Review Application");
    Console.WriteLine("3. Logout");
    Console.Write("\nSelect an option: ");
    
    var choice = Console.ReadLine();
    switch (choice)
    {
        case "1":
            var apps = await apiClient.GetApplicationsAsync();
            if (apps != null && apps.Any())
            {
                Console.WriteLine("\nSubmitted Applications:");
                foreach (var a in apps)
                    Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}");
            }
            else Console.WriteLine("\nNo applications found.");
            WaitForKey();
            break;
        case "2":
            Console.Write("Enter Application ID: ");
            if (!int.TryParse(Console.ReadLine(), out var appId)) break;
            Console.WriteLine("\nSelect Status to Apply:");
            Console.WriteLine("1. Returned");
            Console.WriteLine("2. Reviewed");
            Console.WriteLine("3. Rejected");
            Console.Write("Choice: ");
            var statChoice = Console.ReadLine();
            string status = statChoice switch { "1" => "Returned", "2" => "Reviewed", "3" => "Rejected", _ => "" };
            
            Console.Write("Remarks: ");
            var remarks = Console.ReadLine();
            
            var success = await apiClient.ReviewApplicationAsync(appId, status, remarks);
            Console.WriteLine(success ? "\nReview submitted!" : "\nFailed to review application.");
            WaitForKey();
            break;
        case "3": Logout(); break;
    }
}

async Task ApproverMenuAsync()
{
    Console.WriteLine("\n--- Approver Menu ---");
    Console.WriteLine("1. View Reviewed Applications");
    Console.WriteLine("2. Approve/Reject Application");
    Console.WriteLine("3. View Treasury Balance");
    Console.WriteLine("4. Logout");
    Console.Write("\nSelect an option: ");
    
    var choice = Console.ReadLine();
    switch (choice)
    {
        case "1":
            var apps = await apiClient.GetApplicationsAsync();
            if (apps != null && apps.Any())
            {
                Console.WriteLine("\nReviewed Applications:");
                foreach (var a in apps)
                    Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}");
            }
            else Console.WriteLine("\nNo applications found.");
            WaitForKey();
            break;
        case "2":
            Console.Write("Enter Application ID: ");
            if (!int.TryParse(Console.ReadLine(), out var appId)) break;
            Console.WriteLine("\nSelect Status to Apply:");
            Console.WriteLine("1. Approved");
            Console.WriteLine("2. Rejected");
            Console.Write("Choice: ");
            var statChoice = Console.ReadLine();
            string status = statChoice switch { "1" => "Approved", "2" => "Rejected", _ => "" };
            
            Console.Write("Remarks: ");
            var remarks = Console.ReadLine();
            
            var success = await apiClient.ApproveApplicationAsync(appId, status, remarks);
            Console.WriteLine(success ? "\nApproval processed!" : "\nFailed to process approval.");
            WaitForKey();
            break;
        case "3":
            var bal = await apiClient.GetTreasuryBalanceAsync();
            Console.WriteLine(bal.HasValue ? $"\nTreasury Balance: {bal.Value:C}" : "\nFailed to fetch balance.");
            WaitForKey();
            break;
        case "4": Logout(); break;
    }
}

async Task AdminMenuAsync()
{
    Console.WriteLine("\n--- Admin Menu ---");
    Console.WriteLine("1. View All Applications");
    Console.WriteLine("2. View Treasury Balance");
    Console.WriteLine("3. Create User Account");
    Console.WriteLine("4. Logout");
    Console.Write("\nSelect an option: ");
    
    var choice = Console.ReadLine();
    switch (choice)
    {
        case "1":
            var apps = await apiClient.GetApplicationsAsync();
            if (apps != null && apps.Any())
            {
                Console.WriteLine("\nAll Applications:");
                foreach (var a in apps)
                    Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}");
            }
            else Console.WriteLine("\nNo applications found.");
            WaitForKey();
            break;
        case "2":
            var bal = await apiClient.GetTreasuryBalanceAsync();
            Console.WriteLine(bal.HasValue ? $"\nTreasury Balance: {bal.Value:C}" : "\nFailed to fetch balance.");
            WaitForKey();
            break;
        case "3":
            Console.Write("New Username: ");
            var username = Console.ReadLine() ?? "";
            Console.Write("New Password: ");
            var password = Console.ReadLine() ?? "";
            Console.Write("Role (Applicant, Reviewer, Approver, Admin): ");
            var role = Console.ReadLine() ?? "";
            
            var (success, error) = await apiClient.AdminRegisterUserAsync(username, password, role);
            Console.WriteLine(success ? "\nAccount created successfully!" : $"\nAccount creation failed: {error}");
            WaitForKey();
            break;
        case "4": Logout(); break;
    }
}

void WaitForKey()
{
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}
