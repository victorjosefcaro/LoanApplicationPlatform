using LoanApplicationPlatform.ConsoleApp.Services;
using System.IdentityModel.Tokens.Jwt;

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
            default: Console.WriteLine("\nInvalid option."); Console.ReadKey(); break;
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
    Console.ReadKey();
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
    Console.ReadKey();
}

void Logout()
{
    apiClient.ClearToken();
    currentRole = null;
    Console.WriteLine("\nLogged out successfully.");
    Console.ReadKey();
}

async Task ApplicantMenuAsync()
{
    Console.WriteLine("\n--- Applicant Menu ---");
    Console.WriteLine("1. View My Applications");
    Console.WriteLine("2. Create New Application");
    Console.WriteLine("3. Submit Application");
    Console.WriteLine("4. View Payment Schedules");
    Console.WriteLine("5. Make a Payment");
    Console.WriteLine("6. Logout");
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
            Console.ReadKey();
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
            Console.WriteLine(success ? "\nApplication created as Draft!" : "\nFailed to create application.");
            Console.ReadKey();
            break;
        case "3":
            Console.Write("Enter Application ID to submit: ");
            if (int.TryParse(Console.ReadLine(), out var appId))
            {
                var subSuccess = await apiClient.SubmitApplicationAsync(appId);
                Console.WriteLine(subSuccess ? "\nApplication submitted!" : "\nFailed to submit application.");
            }
            Console.ReadKey();
            break;
        case "4":
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
            Console.ReadKey();
            break;
        case "5":
            Console.Write("Enter Application ID: ");
            if (!int.TryParse(Console.ReadLine(), out var loanId)) break;
            Console.Write("Enter Schedule ID: ");
            if (!int.TryParse(Console.ReadLine(), out var schId)) break;
            Console.Write("Enter Payment Amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out var payAmt)) break;
            
            var paySuccess = await apiClient.SubmitPaymentAsync(loanId, schId, payAmt);
            Console.WriteLine(paySuccess ? "\nPayment successful!" : "\nPayment failed.");
            Console.ReadKey();
            break;
        case "6": Logout(); break;
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
            Console.ReadKey();
            break;
        case "2":
            Console.Write("Enter Application ID: ");
            if (!int.TryParse(Console.ReadLine(), out var appId)) break;
            Console.Write("Status (Returned, Reviewed, Rejected): ");
            var status = Console.ReadLine();
            Console.Write("Remarks: ");
            var remarks = Console.ReadLine();
            
            var success = await apiClient.ReviewApplicationAsync(appId, status ?? "", remarks);
            Console.WriteLine(success ? "\nReview submitted!" : "\nFailed to review application.");
            Console.ReadKey();
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
            Console.ReadKey();
            break;
        case "2":
            Console.Write("Enter Application ID: ");
            if (!int.TryParse(Console.ReadLine(), out var appId)) break;
            Console.Write("Status (Approved, Rejected): ");
            var status = Console.ReadLine();
            Console.Write("Remarks: ");
            var remarks = Console.ReadLine();
            
            var success = await apiClient.ApproveApplicationAsync(appId, status ?? "", remarks);
            Console.WriteLine(success ? "\nApproval processed!" : "\nFailed to process approval.");
            Console.ReadKey();
            break;
        case "3":
            var bal = await apiClient.GetTreasuryBalanceAsync();
            Console.WriteLine(bal.HasValue ? $"\nTreasury Balance: {bal.Value:C}" : "\nFailed to fetch balance.");
            Console.ReadKey();
            break;
        case "4": Logout(); break;
    }
}

async Task AdminMenuAsync()
{
    Console.WriteLine("\n--- Admin Menu ---");
    Console.WriteLine("1. View All Applications");
    Console.WriteLine("2. View Treasury Balance");
    Console.WriteLine("3. Logout");
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
            Console.ReadKey();
            break;
        case "2":
            var bal = await apiClient.GetTreasuryBalanceAsync();
            Console.WriteLine(bal.HasValue ? $"\nTreasury Balance: {bal.Value:C}" : "\nFailed to fetch balance.");
            Console.ReadKey();
            break;
        case "3": Logout(); break;
    }
}
