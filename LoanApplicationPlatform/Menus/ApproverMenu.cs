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
            var apps = await apiClient.GetApplicationsAsync();
            
            switch (choice)
            {
                case "1":
                    var pending = apps?.Where(a => a.Status == "Reviewed").ToList();
                    if (pending != null && pending.Any())
                    {
                        Console.WriteLine("\nApplications Pending Approval:");
                        foreach (var a in pending) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
                        ConsoleHelper.WaitForKey();
                    }
                    else ConsoleHelper.PrintError("No applications found.");
                    break;
                case "2":
                    var approved = apps?.Where(a => a.Status == "Approved").ToList();
                    if (approved != null && approved.Any())
                    {
                        Console.WriteLine("\nApproved Applications (Historical):");
                        foreach (var a in approved) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
                        ConsoleHelper.WaitForKey();
                    }
                    else ConsoleHelper.PrintError("No approved applications found.");
                    break;
                case "3":
                    var rejected = apps?.Where(a => a.Status == "Rejected").ToList();
                    if (rejected != null && rejected.Any())
                    {
                        Console.WriteLine("\nRejected Applications (Historical):");
                        foreach (var a in rejected) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
                        ConsoleHelper.WaitForKey();
                    }
                    else ConsoleHelper.PrintError("No rejected applications found.");
                    break;
                case "4":
                    var toApprove = apps?.Where(a => a.Status == "Reviewed").ToList();
                    if (toApprove == null || !toApprove.Any()) { ConsoleHelper.PrintError("No applications to approve."); break; }
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
