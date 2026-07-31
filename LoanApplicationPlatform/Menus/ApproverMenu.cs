using System;
using System.Linq;
using System.Threading.Tasks;
using LoanApplicationPlatform.ConsoleApp.Services;

namespace LoanApplicationPlatform.ConsoleApp.Menus
{
    public static class ApproverMenu
    {
        public static async Task RunAsync(LoanApiClient apiClient, Action logoutCallback)
        {
            ConsoleHelper.PrintHeader("Approver Menu");
            Console.WriteLine("1. View Applications Pending Approval");
            Console.WriteLine("2. View Approved Applications");
            Console.WriteLine("3. View Rejected Applications");
            Console.WriteLine("4. Process an Application");
            Console.WriteLine("5. View Treasury Balance");
            Console.WriteLine("6. Logout");
            Console.Write("\nSelect an option: ");
            
            var choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    var pendingApps = await apiClient.GetApplicationsAsync(1, 1000, status: "Reviewed");
                    var pending = pendingApps?.Items?.ToList();
                    if (pending != null && pending.Any())
                    {
                        Console.WriteLine("\nApplications Pending Approval:");
                        foreach (var a in pending) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
                        ConsoleHelper.WaitForKey();
                    }
                    else ConsoleHelper.PrintError("No applications found.");
                    break;
                case "2":
                    var approvedApps = await apiClient.GetApplicationsAsync(1, 1000, status: "Approved");
                    var approved = approvedApps?.Items?.ToList();
                    if (approved != null && approved.Any())
                    {
                        Console.WriteLine("\nApproved Applications (Historical):");
                        foreach (var a in approved) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
                        ConsoleHelper.WaitForKey();
                    }
                    else ConsoleHelper.PrintError("No approved applications found.");
                    break;
                case "3":
                    var rejectedApps = await apiClient.GetApplicationsAsync(1, 1000, status: "Rejected");
                    var rejected = rejectedApps?.Items?.ToList();
                    if (rejected != null && rejected.Any())
                    {
                        Console.WriteLine("\nRejected Applications (Historical):");
                        foreach (var a in rejected) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
                        ConsoleHelper.WaitForKey();
                    }
                    else ConsoleHelper.PrintError("No rejected applications found.");
                    break;
                case "4":
                    var toApproveApps = await apiClient.GetApplicationsAsync(1, 1000, status: "Reviewed");
                    var toApprove = toApproveApps?.Items?.ToList();
                    if (toApprove == null || !toApprove.Any()) { ConsoleHelper.PrintError("No applications to approve."); break; }
                    Console.WriteLine("\nApplications to Approve:");
                    foreach (var a in toApprove) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
                    
                    Console.Write("\nEnter Application ID: ");
                    if (!int.TryParse(Console.ReadLine(), out var appId)) { ConsoleHelper.PrintError("Invalid ID."); break; }
                    Console.WriteLine("\nSelect Status to Apply:");
                    Console.WriteLine("1. Approved");
                    Console.WriteLine("2. Rejected");
                    Console.WriteLine("3. Returned");
                    Console.Write("Choice: ");
                    var statChoice = Console.ReadLine();
                    string status = statChoice switch { "1" => "Approved", "2" => "Rejected", "3" => "Returned", _ => "" };
                    
                    Console.Write("Remarks: ");
                    var remarks = Console.ReadLine();
                    
                    var success = await apiClient.ApproveApplicationAsync(appId, status, remarks);
                    if (success) ConsoleHelper.PrintSuccess("Approval processed!");
                    else ConsoleHelper.PrintError("Failed to process approval.");
                    break;
                case "5":
                    var bal = await apiClient.GetTreasuryBalanceAsync();
                    if (bal.HasValue) ConsoleHelper.PrintSuccess($"Treasury Balance: {bal.Value:C}");
                    else ConsoleHelper.PrintError("Failed to fetch balance.");
                    break;
                case "6":
                    logoutCallback();
                    break;
                default:
                    ConsoleHelper.PrintError("Invalid option.");
                    break;
            }
        }
    }
}
