using System;
using System.Linq;
using System.Threading.Tasks;
using LoanApplicationPlatform.ConsoleApp.Services;

namespace LoanApplicationPlatform.ConsoleApp.Menus
{
    public static class ApplicantMenu
    {
        public static async Task RunAsync(LoanApiClient apiClient, Action logoutCallback)
        {
            ConsoleHelper.PrintHeader("Applicant Menu");
            Console.WriteLine("1. View My Applications");
            Console.WriteLine("2. Create & Submit Application");
            Console.WriteLine("3. Edit & Resubmit Returned Application");
            Console.WriteLine("4. Cancel Application");
            Console.WriteLine("5. View Payment Schedules");
            Console.WriteLine("6. Make a Payment");
            Console.WriteLine("7. Logout");
            Console.Write("\nSelect an option: ");
            
            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    await ViewMyApplications(apiClient);
                    break;
                case "2":
                    await CreateApplication(apiClient);
                    break;
                case "3":
                    await EditReturnedApplication(apiClient);
                    break;
                case "4":
                    await CancelApplication(apiClient);
                    break;
                case "5":
                    await ViewPaymentSchedules(apiClient);
                    break;
                case "6":
                    await MakePayment(apiClient);
                    break;
                case "7":
                    logoutCallback();
                    break;
                default:
                    ConsoleHelper.PrintError("Invalid option.");
                    break;
            }
        }

        private static async Task ViewMyApplications(LoanApiClient apiClient)
        {
            await ConsolePaginator.PaginateAsync(
                "My Applications",
                new[] { "ID", "Amount", "Term", "Status", "Remarks" },
                (ApplicationDto a) => new[] { a.Id.ToString(), a.Amount.ToString("C"), $"{a.TermInMonths} mos", a.Status, a.Remarks ?? "" },
                (page, size) => apiClient.GetApplicationsAsync(page, size)
            );
        }

        private static async Task CreateApplication(LoanApiClient apiClient)
        {
            Console.Write("Applicant Name: ");
            var name = Console.ReadLine();
            Console.Write("Amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out var amt)) { ConsoleHelper.PrintError("Invalid amount."); return; }
            Console.Write("Term (months): ");
            if (!int.TryParse(Console.ReadLine(), out var term)) { ConsoleHelper.PrintError("Invalid term."); return; }
            Console.Write("Monthly Income: ");
            if (!decimal.TryParse(Console.ReadLine(), out var inc)) { ConsoleHelper.PrintError("Invalid income."); return; }
            Console.Write("Purpose: ");
            var purpose = Console.ReadLine();
            
            var success = await apiClient.CreateApplicationAsync(new { ApplicantName = name, Amount = amt, TermInMonths = term, MonthlyIncome = inc, Purpose = purpose });
            if (success) ConsoleHelper.PrintSuccess("Application successfully created and submitted!");
            else ConsoleHelper.PrintError("Failed to create application (check income requirements).");
        }

        private static async Task EditReturnedApplication(LoanApiClient apiClient)
        {
            var retApps = await apiClient.GetApplicationsAsync(1, 1000);
            var returned = retApps?.Items?.Where(a => a.Status == "Returned").ToList();
            if (returned == null || !returned.Any()) {
                ConsoleHelper.PrintError("You have no returned applications to edit.");
                return;
            }
            Console.WriteLine("\nReturned Applications:");
            foreach (var a in returned) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Remarks: {a.Remarks}");
            
            Console.Write("\nEnter Application ID to edit: ");
            if (int.TryParse(Console.ReadLine(), out var editId))
            {
                if (!returned.Any(a => a.Id == editId)) { ConsoleHelper.PrintError("Invalid ID."); return; }
                
                Console.Write("Updated Applicant Name: ");
                var ename = Console.ReadLine();
                Console.Write("Updated Amount: ");
                if (!decimal.TryParse(Console.ReadLine(), out var eamt)) { ConsoleHelper.PrintError("Invalid amount."); return; }
                Console.Write("Updated Term (months): ");
                if (!int.TryParse(Console.ReadLine(), out var eterm)) { ConsoleHelper.PrintError("Invalid term."); return; }
                Console.Write("Updated Monthly Income: ");
                if (!decimal.TryParse(Console.ReadLine(), out var einc)) { ConsoleHelper.PrintError("Invalid income."); return; }
                Console.Write("Updated Purpose: ");
                var epurpose = Console.ReadLine();
                
                var upSuccess = await apiClient.UpdateApplicationAsync(editId, new { ApplicantName = ename, Amount = eamt, TermInMonths = eterm, MonthlyIncome = einc, Purpose = epurpose });
                if (upSuccess) {
                    ConsoleHelper.PrintSuccess("Application successfully updated and resubmitted!");
                } else {
                    ConsoleHelper.PrintError("Failed to update and resubmit application (check income requirements).");
                }
            }
        }

        private static async Task ViewPendingActionItems(LoanApiClient apiClient)
        {
            var apps = await apiClient.GetApplicationsAsync(1, 1000);
            var pendingInfo = apps?.Items?.Where(a => a.Status == "PendingInfo").ToList();
            if (pendingInfo == null || !pendingInfo.Any())
            {
                ConsoleHelper.PrintSuccess("No pending action items!");
                return;
            }
            
            Console.WriteLine("\nApplications Requiring Action:");
            foreach (var a in pendingInfo)
                Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}");
            
            Console.Write("\nEnter Application ID: ");
            if (int.TryParse(Console.ReadLine(), out var pid))
            {
                // Action implementation logic here...
            }
        }

        private static async Task ViewPaymentSchedules(LoanApiClient apiClient)
        {
            var apps = await apiClient.GetApplicationsAsync(1, 1000);
            if (apps == null || apps.Items == null || !apps.Items.Any())
            {
                ConsoleHelper.PrintError("You have no applications.");
                return;
            }
            
            Console.WriteLine("\nYour Applications:");
            foreach (var a in apps.Items)
                Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}");
            
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
                    ConsoleHelper.WaitForKey();
                }
                else ConsoleHelper.PrintError("No payment schedules found.");
            }
        }

        private static async Task MakePayment(LoanApiClient apiClient)
        {
            var allApps = await apiClient.GetApplicationsAsync(1, 1000);
            if (allApps == null || allApps.Items == null || !allApps.Items.Any()) { ConsoleHelper.PrintError("You have no applications."); return; }
            Console.WriteLine("\nYour Applications:");
            foreach (var a in allApps.Items) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}");
            
            Console.Write("\nEnter Application ID: ");
            if (!int.TryParse(Console.ReadLine(), out var loanId)) { ConsoleHelper.PrintError("Invalid ID."); return; }
            
            var sch = await apiClient.GetPaymentSchedulesAsync(loanId);
            if (sch == null || !sch.Any(s => s.Status != "Paid" && s.Status != "PaymentSubmitted" && s.Status != "Payment Submitted")) { ConsoleHelper.PrintError("No pending schedules to pay."); return; }
            Console.WriteLine("\nPending Schedules:");
            foreach (var s in sch.Where(s => s.Status != "Paid" && s.Status != "PaymentSubmitted" && s.Status != "Payment Submitted")) Console.WriteLine($"- SchID: {s.Id}, Due: {s.DueDate:yyyy-MM-dd}, Amount: {s.AmountDue:C}");
            
            Console.Write("\nEnter Schedule ID to notify payment sent: ");
            if (!int.TryParse(Console.ReadLine(), out var schId)) { ConsoleHelper.PrintError("Invalid Schedule ID."); return; }
            
            var targetSch = sch.FirstOrDefault(s => s.Id == schId);
            var remainingDue = targetSch != null ? (targetSch.AmountDue - targetSch.AmountPaid) : 0m;
            Console.Write($"Enter Payment Amount to submit [default {remainingDue:C}]: ");
            var amtInput = Console.ReadLine()?.Trim();
            decimal payAmount;
            if (string.IsNullOrEmpty(amtInput))
            {
                payAmount = remainingDue;
            }
            else
            {
                if (!decimal.TryParse(amtInput, out payAmount) || payAmount <= 0) { ConsoleHelper.PrintError("Invalid payment amount."); return; }
            }

            var paySuccess = await apiClient.SubmitPaymentAsync(loanId, schId, payAmount);
            if (paySuccess) ConsoleHelper.PrintSuccess($"Payment notification for {payAmount:C} submitted successfully! Waiting for Admin to post.");
            else ConsoleHelper.PrintError("Payment notification failed.");
        }

        private static async Task CancelApplication(LoanApiClient apiClient)
        {
            var apps = await apiClient.GetApplicationsAsync(1, 1000);
            var cancellable = apps?.Items?.Where(a => a.Status == "Submitted" || a.Status == "Returned").ToList();
            if (cancellable == null || !cancellable.Any())
            {
                ConsoleHelper.PrintError("You have no applications eligible for cancellation.");
                return;
            }

            Console.WriteLine("\nApplications Eligible for Cancellation:");
            foreach (var a in cancellable)
                Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}");

            Console.Write("\nEnter Application ID to Cancel: ");
            if (!int.TryParse(Console.ReadLine(), out var cancelId))
            {
                ConsoleHelper.PrintError("Invalid ID.");
                return;
            }

            if (!cancellable.Any(a => a.Id == cancelId))
            {
                ConsoleHelper.PrintError("Invalid application selection.");
                return;
            }

            var success = await apiClient.CancelApplicationAsync(cancelId);
            if (success) ConsoleHelper.PrintSuccess("Application cancelled successfully!");
            else ConsoleHelper.PrintError("Failed to cancel application.");
        }
    }
}
