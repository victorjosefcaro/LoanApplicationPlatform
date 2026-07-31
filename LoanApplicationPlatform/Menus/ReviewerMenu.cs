using System;
using System.Linq;
using System.Threading.Tasks;
using LoanApplicationPlatform.ConsoleApp.Services;

namespace LoanApplicationPlatform.ConsoleApp.Menus
{
    public static class ReviewerMenu
    {
        public static async Task RunAsync(LoanApiClient apiClient, Action logoutCallback)
        {
            ConsoleHelper.PrintHeader("Reviewer Menu");
            Console.WriteLine("1. View Applications Pending Review");
            Console.WriteLine("2. View Approved Applications");
            Console.WriteLine("3. View Rejected Applications");
            Console.WriteLine("4. Process an Application");
            Console.WriteLine("5. Logout");
            Console.Write("\nSelect an option: ");
            
            var choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    var pendingApps = await apiClient.GetApplicationsAsync(1, 1000, status: "Submitted");
                    var pending = pendingApps?.Items?.ToList();
                    if (pending != null && pending.Any())
                    {
                        Console.WriteLine("\nApplications Pending Review:");
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
                    var toReviewApps = await apiClient.GetApplicationsAsync(1, 1000, status: "Submitted");
                    var toReview = toReviewApps?.Items?.ToList();
                    if (toReview == null || !toReview.Any()) { ConsoleHelper.PrintError("No applications to review."); break; }
                    Console.WriteLine("\nApplications to Review:");
                    foreach (var a in toReview) Console.WriteLine($"- ID: {a.Id}, Amount: {a.Amount:C}, Status: {a.Status}, Remarks: {a.Remarks}");
                    
                    Console.Write("\nEnter Application ID: ");
                    if (!int.TryParse(Console.ReadLine(), out var appId)) { ConsoleHelper.PrintError("Invalid ID."); break; }
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
                    if (success) ConsoleHelper.PrintSuccess("Review submitted!");
                    else ConsoleHelper.PrintError("Failed to review application.");
                    break;
                case "5":
                    logoutCallback();
                    break;
                default:
                    ConsoleHelper.PrintError("Invalid option.");
                    break;
            }
        }
    }
}
