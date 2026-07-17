using LoanApplicationPlatform.ConsoleApp.Services;
using System.IdentityModel.Tokens.Jwt;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var apiClient = new LoanApiClient("https://localhost:7103");
string? currentRole = null;

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
    Console.WriteLine("3. Edit & Resubmit Returned Application");
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
                var activeApps = apps.OrderByDescending(a => a.CreatedAt).ToList();
                Console.WriteLine("\nYour Applications:");
                foreach (var a in activeApps)
                {
                    Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Term: {a.TermInMonths} mos, Status: {a.Status}, Remarks: {a.Remarks}");
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
            var retApps = await apiClient.GetApplicationsAsync();
            var returned = retApps?.Where(a => a.Status == "Returned").ToList();
            if (returned == null || !returned.Any()) {
                Console.WriteLine("\nYou have no returned applications to edit.");
                WaitForKey();
                break;
            }
            Console.WriteLine("\nReturned Applications:");
            foreach (var a in returned) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Remarks: {a.Remarks}");
            
            Console.Write("\nEnter Application ID to edit: ");
            if (int.TryParse(Console.ReadLine(), out var editId))
            {
                if (!returned.Any(a => a.Id == editId)) { Console.WriteLine("Invalid ID."); WaitForKey(); break; }
                
                Console.Write("Updated Applicant Name: ");
                var ename = Console.ReadLine();
                Console.Write("Updated Amount: ");
                if (!decimal.TryParse(Console.ReadLine(), out var eamt)) break;
                Console.Write("Updated Term (months): ");
                if (!int.TryParse(Console.ReadLine(), out var eterm)) break;
                Console.Write("Updated Monthly Income: ");
                if (!decimal.TryParse(Console.ReadLine(), out var einc)) break;
                Console.Write("Updated Purpose: ");
                var epurpose = Console.ReadLine();
                
                var upSuccess = await apiClient.UpdateApplicationAsync(editId, new { ApplicantName = ename, Amount = eamt, TermInMonths = eterm, MonthlyIncome = einc, Purpose = epurpose });
                if (upSuccess) {
                    var subSuccess = await apiClient.SubmitApplicationAsync(editId);
                    Console.WriteLine(subSuccess ? "\nApplication successfully updated and resubmitted!" : "\nUpdated successfully, but failed to resubmit (check income requirements).");
                } else {
                    Console.WriteLine("\nFailed to update application.");
                }
            }
            WaitForKey();
            break;
        case "4":
            var appList = await apiClient.GetApplicationsAsync();
            if (appList == null || !appList.Any()) { Console.WriteLine("\nYou have no applications."); WaitForKey(); break; }
            Console.WriteLine("\nYour Applications:");
            foreach (var a in appList) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}");
            
            Console.Write("\nEnter Application ID: ");
            if (int.TryParse(Console.ReadLine(), out var pid))
            {
                var schedules = await apiClient.GetPaymentSchedulesAsync(pid);
                if (schedules != null && schedules.Any())
                {
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
        case "5":
            var allApps = await apiClient.GetApplicationsAsync();
            if (allApps == null || !allApps.Any()) { Console.WriteLine("\nYou have no applications."); WaitForKey(); break; }
            Console.WriteLine("\nYour Applications:");
            foreach (var a in allApps) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}");
            
            Console.Write("\nEnter Application ID: ");
            if (!int.TryParse(Console.ReadLine(), out var loanId)) break;
            
            var sch = await apiClient.GetPaymentSchedulesAsync(loanId);
            if (sch == null || !sch.Any(s => s.Status != "Paid" && s.Status != "Payment Submitted")) { Console.WriteLine("\nNo pending schedules to pay."); WaitForKey(); break; }
            Console.WriteLine("\nPending Schedules:");
            foreach (var s in sch.Where(s => s.Status != "Paid" && s.Status != "Payment Submitted")) Console.WriteLine($"- SchID: {s.Id}, Due: {s.DueDate:yyyy-MM-dd}, Amount: {s.AmountDue:C}");
            
            Console.Write("\nEnter Schedule ID to notify payment sent: ");
            if (!int.TryParse(Console.ReadLine(), out var schId)) break;
            
            var paySuccess = await apiClient.SubmitPaymentAsync(loanId, schId);
            Console.WriteLine(paySuccess ? "\nPayment notified successfully! Waiting for Admin to post." : "\nPayment notification failed.");
            WaitForKey();
            break;
        case "6": Logout(); break;
    }
}

async Task ReviewerMenuAsync()
{
    Console.WriteLine("\n--- Reviewer Menu ---");
    Console.WriteLine("1. View Applications Pending Review");
    Console.WriteLine("2. View Approved Applications");
    Console.WriteLine("3. View Rejected Applications");
    Console.WriteLine("4. Process an Application");
    Console.WriteLine("5. Logout");
    Console.Write("\nSelect an option: ");
    
    var choice = Console.ReadLine();
    var apps = await apiClient.GetApplicationsAsync();
    switch (choice)
    {
        case "1":
            var pending = apps?.Where(a => a.Status == "Submitted").ToList();
            if (pending != null && pending.Any())
            {
                Console.WriteLine("\nApplications Pending Review:");
                foreach (var a in pending) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
            }
            else Console.WriteLine("\nNo applications found.");
            WaitForKey();
            break;
        case "2":
            var approved = apps?.Where(a => a.Status == "Approved").ToList();
            if (approved != null && approved.Any())
            {
                Console.WriteLine("\nApproved Applications (Historical):");
                foreach (var a in approved) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
            }
            else Console.WriteLine("\nNo approved applications found.");
            WaitForKey();
            break;
        case "3":
            var rejected = apps?.Where(a => a.Status == "Rejected").ToList();
            if (rejected != null && rejected.Any())
            {
                Console.WriteLine("\nRejected Applications (Historical):");
                foreach (var a in rejected) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
            }
            else Console.WriteLine("\nNo rejected applications found.");
            WaitForKey();
            break;
        case "4":
            var toReview = apps?.Where(a => a.Status == "Submitted").ToList();
            if (toReview == null || !toReview.Any()) { Console.WriteLine("\nNo applications to review."); WaitForKey(); break; }
            Console.WriteLine("\nApplications to Review:");
            foreach (var a in toReview) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
            
            Console.Write("\nEnter Application ID: ");
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
        case "5": Logout(); break;
    }
}

async Task ApproverMenuAsync()
{
    Console.WriteLine("\n--- Approver Menu ---");
    Console.WriteLine("1. View Applications Pending Approval");
    Console.WriteLine("2. View Approved Applications");
    Console.WriteLine("3. View Rejected Applications");
    Console.WriteLine("4. Process an Application");
    Console.WriteLine("5. View Treasury Balance");
    Console.WriteLine("6. Logout");
    Console.Write("\nSelect an option: ");
    
    var choice = Console.ReadLine();
    var apps = await apiClient.GetApplicationsAsync();
    switch (choice)
    {
        case "1":
            var pending = apps?.Where(a => a.Status == "Reviewed").ToList();
            if (pending != null && pending.Any())
            {
                Console.WriteLine("\nApplications Pending Approval:");
                foreach (var a in pending) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
            }
            else Console.WriteLine("\nNo applications found.");
            WaitForKey();
            break;
        case "2":
            var approved = apps?.Where(a => a.Status == "Approved").ToList();
            if (approved != null && approved.Any())
            {
                Console.WriteLine("\nApproved Applications (Historical):");
                foreach (var a in approved) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
            }
            else Console.WriteLine("\nNo approved applications found.");
            WaitForKey();
            break;
        case "3":
            var rejected = apps?.Where(a => a.Status == "Rejected").ToList();
            if (rejected != null && rejected.Any())
            {
                Console.WriteLine("\nRejected Applications (Historical):");
                foreach (var a in rejected) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
            }
            else Console.WriteLine("\nNo rejected applications found.");
            WaitForKey();
            break;
        case "4":
            var toApprove = apps?.Where(a => a.Status == "Reviewed").ToList();
            if (toApprove == null || !toApprove.Any()) { Console.WriteLine("\nNo applications to approve."); WaitForKey(); break; }
            Console.WriteLine("\nApplications to Approve:");
            foreach (var a in toApprove) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
            
            Console.Write("\nEnter Application ID: ");
            if (!int.TryParse(Console.ReadLine(), out var appId)) break;
            Console.WriteLine("\nSelect Status to Apply:");
            Console.WriteLine("1. Returned");
            Console.WriteLine("2. Approved");
            Console.WriteLine("3. Rejected");
            Console.Write("Choice: ");
            var statChoice = Console.ReadLine();
            string status = statChoice switch { "1" => "Returned", "2" => "Approved", "3" => "Rejected", _ => "" };
            
            Console.Write("Remarks: ");
            var remarks = Console.ReadLine();
            
            var success = await apiClient.ApproveApplicationAsync(appId, status, remarks);
            Console.WriteLine(success ? "\nApproval processed!" : "\nFailed to process approval.");
            WaitForKey();
            break;
        case "5":
            var bal = await apiClient.GetTreasuryBalanceAsync();
            Console.WriteLine(bal.HasValue ? $"\nTreasury Balance: {bal.Value:C}" : "\nFailed to fetch balance.");
            WaitForKey();
            break;
        case "6": Logout(); break;
    }
}

async Task AdminMenuAsync()
{
    Console.WriteLine("\n--- Admin Menu ---");
    Console.WriteLine("1. View All Applications");
    Console.WriteLine("2. View Treasury Balance");
    Console.WriteLine("3. Deposit to Treasury");
    Console.WriteLine("4. Create User Account");
    Console.WriteLine("5. Release Funds for Approved Application");
    Console.WriteLine("6. Post Pending Payments");
    Console.WriteLine("7. Logout");
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
                    Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
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
            Console.Write("Enter Amount to Deposit: ");
            if (!decimal.TryParse(Console.ReadLine(), out var depositAmt) || depositAmt <= 0)
            {
                Console.WriteLine("\nInvalid deposit amount.");
                WaitForKey();
                break;
            }
            var depSuccess = await apiClient.DepositToTreasuryAsync(depositAmt);
            Console.WriteLine(depSuccess ? "\nSuccessfully deposited funds to Treasury!" : "\nFailed to deposit funds.");
            WaitForKey();
            break;
        case "4":
            Console.Write("New Username: ");
            var username = Console.ReadLine() ?? "";
            Console.Write("New Password: ");
            var password = Console.ReadLine() ?? "";
            Console.WriteLine("\nSelect Role:");
            Console.WriteLine("1. Applicant");
            Console.WriteLine("2. Reviewer");
            Console.WriteLine("3. Approver");
            Console.WriteLine("4. Admin");
            Console.Write("Choice: ");
            var roleChoice = Console.ReadLine();
            var role = roleChoice switch { "1" => "Applicant", "2" => "Reviewer", "3" => "Approver", "4" => "Admin", _ => "" };
            
            var (success, error) = await apiClient.AdminRegisterUserAsync(username, password, role);
            Console.WriteLine(success ? "\nAccount created successfully!" : $"\nAccount creation failed: {error}");
            WaitForKey();
            break;
        case "5":
            var appList = await apiClient.GetApplicationsAsync();
            var approvedList = appList?.Where(a => a.Status == "Approved").ToList();
            if (approvedList == null || !approvedList.Any())
            {
                Console.WriteLine("\nNo approved applications waiting for fund release.");
                WaitForKey();
                break;
            }
            Console.WriteLine("\nApproved Applications Pending Release:");
            foreach (var a in approvedList)
                Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Remarks: {a.Remarks}");
            
            Console.Write("\nEnter Application ID to release funds: ");
            if (!int.TryParse(Console.ReadLine(), out var releaseId)) break;
            
            var relSuccess = await apiClient.ReleaseFundsAsync(releaseId);
            Console.WriteLine(relSuccess ? "\nFunds successfully released! Payment schedules generated." : "\nFailed to release funds (check treasury balance).");
            WaitForKey();
            break;
        case "6":
            var allAppsForPay = await apiClient.GetApplicationsAsync();
            if (allAppsForPay == null || !allAppsForPay.Any()) { Console.WriteLine("\nNo applications."); WaitForKey(); break; }
            
            Console.WriteLine("\nAll Applications:");
            foreach (var a in allAppsForPay) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}");
            
            Console.Write("\nEnter Application ID to check schedules: ");
            if (!int.TryParse(Console.ReadLine(), out var pLoanId)) break;
            
            var pSch = await apiClient.GetPaymentSchedulesAsync(pLoanId);
            var submittedSchs = pSch?.Where(s => s.Status == "Payment Submitted" || s.Status == "Partially Paid").ToList();
            if (submittedSchs == null || !submittedSchs.Any()) { Console.WriteLine("\nNo submitted payments for this application."); WaitForKey(); break; }
            
            Console.WriteLine("\nSubmitted Schedules:");
            foreach (var s in submittedSchs)
                Console.WriteLine($"- SchID: {s.Id}, Due: {s.DueDate:yyyy-MM-dd}, AmountDue: {s.AmountDue:C}, AmountPaid: {s.AmountPaid:C}, Status: {s.Status}");
                
            Console.Write("\nEnter Schedule ID to post: ");
            if (!int.TryParse(Console.ReadLine(), out var pSchId)) break;
            Console.Write("Enter Verified Payment Amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out var verifiedAmt)) break;
            
            var postSuccess = await apiClient.PostPaymentAsync(pLoanId, pSchId, verifiedAmt);
            Console.WriteLine(postSuccess ? "\nPayment posted to treasury successfully!" : "\nFailed to post payment.");
            WaitForKey();
            break;
        case "7": Logout(); break;
    }
}

void WaitForKey()
{
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}
