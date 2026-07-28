using System;
using System.Linq;
using System.Threading.Tasks;
using LoanApplicationPlatform.ConsoleApp.Services;

namespace LoanApplicationPlatform.ConsoleApp.Menus
{
    public static class AdminMenu
    {
        public static async Task RunAsync(LoanApiClient apiClient, Action logoutCallback)
        {
            ConsoleHelper.PrintHeader("Admin Menu");
            Console.WriteLine("1. View All Applications");
            Console.WriteLine("2. View Treasury Balance");
            Console.WriteLine("3. Deposit to Treasury");
            Console.WriteLine("4. View Transaction Ledger");
            Console.WriteLine("5. Create User Account");
            Console.WriteLine("6. Release Funds for Approved Application");
            Console.WriteLine("7. Post Pending Payments");
            Console.WriteLine("8. Logout");
            Console.Write("\nSelect an option: ");
            
            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    await ViewApplications(apiClient);
                    break;
                case "2":
                    await ViewTreasury(apiClient);
                    break;
                case "3":
                    await DepositTreasury(apiClient);
                    break;
                case "4":
                    await ViewLedger(apiClient);
                    break;
                case "5":
                    await CreateUser(apiClient);
                    break;
                case "6":
                    await ReleaseFunds(apiClient);
                    break;
                case "7":
                    await PostPayments(apiClient);
                    break;
                case "8":
                    logoutCallback();
                    break;
                default:
                    ConsoleHelper.PrintError("Invalid option.");
                    break;
            }
        }

        private static async Task ViewApplications(LoanApiClient apiClient)
        {
            await ConsolePaginator.PaginateAsync(
                "All Applications",
                new[] { "ID", "Amount", "Status", "Remarks" },
                (ApplicationDto a) => new[] { a.Id.ToString(), a.Amount.ToString("C"), a.Status, a.Remarks ?? "" },
                (page, size) => apiClient.GetApplicationsAsync(page, size)
            );
        }

        private static async Task ViewTreasury(LoanApiClient apiClient)
        {
            var bal = await apiClient.GetTreasuryBalanceAsync();
            if (bal.HasValue) ConsoleHelper.PrintSuccess($"Treasury Balance: {bal.Value:C}");
            else ConsoleHelper.PrintError("Failed to fetch balance.");
        }

        private static async Task DepositTreasury(LoanApiClient apiClient)
        {
            Console.Write("Enter Amount to Deposit: ");
            if (!decimal.TryParse(Console.ReadLine(), out var depositAmt) || depositAmt <= 0)
            {
                ConsoleHelper.PrintError("Invalid deposit amount.");
                return;
            }
            var depSuccess = await apiClient.DepositToTreasuryAsync(depositAmt);
            if (depSuccess) ConsoleHelper.PrintSuccess("Successfully deposited funds to Treasury!");
            else ConsoleHelper.PrintError("Failed to deposit funds.");
        }

        private static async Task ViewLedger(LoanApiClient apiClient)
        {
            await ConsolePaginator.PaginateAsync(
                "Treasury Transaction Ledger",
                new[] { "ID", "Date (UTC)", "Amount", "Type", "Ref ID" },
                (TreasuryTransactionDto t) => new[] { t.Id.ToString(), t.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss"), t.Amount.ToString("C"), t.Type, t.ReferenceId?.ToString() ?? "N/A" },
                (page, size) => apiClient.GetTreasuryTransactionsAsync(page, size)
            );
        }

        private static async Task CreateUser(LoanApiClient apiClient)
        {
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
            
            Console.WriteLine("\nSelect Tenant:");
            Console.WriteLine("1. Default Lending Co (Tenant ID: 1)");
            Console.WriteLine("2. Acme Finance (Tenant ID: 2)");
            Console.Write("Choice (1 or 2, default 1): ");
            var tenantChoice = Console.ReadLine()?.Trim();
            int tenantId = tenantChoice == "2" ? 2 : 1;

            var (success, error) = await apiClient.AdminRegisterUserAsync(username, password, role, tenantId);
            if (success) ConsoleHelper.PrintSuccess($"Account created successfully for Tenant {tenantId}!");
            else ConsoleHelper.PrintError($"Account creation failed: {error}");
        }

        private static async Task ReleaseFunds(LoanApiClient apiClient)
        {
            // For picking, we can fetch a large page just to display quickly, or let user input ID.
            var appList = await apiClient.GetApplicationsAsync(1, 1000);
            var approvedList = appList?.Items?.Where(a => a.Status == "Approved").ToList();
            if (approvedList == null || !approvedList.Any())
            {
                ConsoleHelper.PrintError("No approved applications waiting for fund release.");
                return;
            }
            Console.WriteLine("\nApproved Applications Pending Release:");
            foreach (var a in approvedList)
                Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Remarks: {a.Remarks}");
            
            Console.Write("\nEnter Application ID to release funds: ");
            if (!int.TryParse(Console.ReadLine(), out var releaseId)) { ConsoleHelper.PrintError("Invalid ID."); return; }
            
            var relSuccess = await apiClient.ReleaseFundsAsync(releaseId);
            if (relSuccess) ConsoleHelper.PrintSuccess("Funds successfully released! Payment schedules generated.");
            else ConsoleHelper.PrintError("Failed to release funds (check treasury balance).");
        }

        private static async Task PostPayments(LoanApiClient apiClient)
        {
            var allAppsForPay = await apiClient.GetApplicationsAsync(1, 1000);
            if (allAppsForPay == null || allAppsForPay.Items == null || !allAppsForPay.Items.Any()) { ConsoleHelper.PrintError("No applications."); return; }
            
            Console.WriteLine("\nAll Applications (First 1000):");
            foreach (var a in allAppsForPay.Items) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}");
            
            Console.Write("\nEnter Application ID to check schedules: ");
            if (!int.TryParse(Console.ReadLine(), out var pLoanId)) { ConsoleHelper.PrintError("Invalid ID."); return; }
            
            var pSch = await apiClient.GetPaymentSchedulesAsync(pLoanId);
            var submittedSchs = pSch?.Where(s => s.Status == "PaymentSubmitted" || s.Status == "Payment Submitted" || s.Status == "PartiallyPaid" || s.Status == "Partially Paid").ToList();
            if (submittedSchs == null || !submittedSchs.Any()) { ConsoleHelper.PrintError("No submitted payments for this application."); return; }
            
            Console.WriteLine("\nSubmitted Schedules:");
            foreach (var s in submittedSchs)
            {
                var claimedStr = s.SubmittedAmount.HasValue ? s.SubmittedAmount.Value.ToString("C") : "N/A";
                Console.WriteLine($"- SchID: {s.Id}, Due: {s.DueDate:yyyy-MM-dd}, AmountDue: {s.AmountDue:C}, AmountPaid: {s.AmountPaid:C}, Applicant Claimed: {claimedStr}, Status: {s.Status}");
            }
                
            Console.Write("\nEnter Schedule ID to post: ");
            if (!int.TryParse(Console.ReadLine(), out var pSchId)) { ConsoleHelper.PrintError("Invalid Schedule ID."); return; }
            
            var targetSch = submittedSchs.FirstOrDefault(s => s.Id == pSchId);
            var defaultAmt = targetSch?.SubmittedAmount ?? (targetSch != null ? (targetSch.AmountDue - targetSch.AmountPaid) : 0m);
            Console.Write($"Enter Verified Payment Amount [default {defaultAmt:C}]: ");
            var verifiedAmtInput = Console.ReadLine()?.Trim();
            decimal verifiedAmt;
            if (string.IsNullOrEmpty(verifiedAmtInput))
            {
                verifiedAmt = defaultAmt;
            }
            else
            {
                if (!decimal.TryParse(verifiedAmtInput, out verifiedAmt) || verifiedAmt <= 0) { ConsoleHelper.PrintError("Invalid amount."); return; }
            }

            var postSuccess = await apiClient.PostPaymentAsync(pLoanId, pSchId, verifiedAmt);
            if (postSuccess) ConsoleHelper.PrintSuccess("Payment posted to treasury successfully!");
            else ConsoleHelper.PrintError("Failed to post payment.");
        }
    }
}
