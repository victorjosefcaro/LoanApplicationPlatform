using System;
using System.Threading.Tasks;
using LoanApplicationPlatform.ConsoleApp.Models;
using LoanApplicationPlatform.ConsoleApp.Services;

namespace LoanApplicationPlatform.ConsoleApp.Menus
{
    public static class ConsolePaginator
    {
        public static async Task PaginateAsync<T>(
            string title,
            string[] headers,
            Func<T, string[]> rowFormatter,
            Func<int, int, Task<PagedResponse<T>?>> fetchPageFunc,
            int pageSize = 10)
        {
            int currentPage = 1;
            while (true)
            {
                var response = await fetchPageFunc(currentPage, pageSize);
                
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("=====================================");
                Console.WriteLine($"  {title.ToUpper()}");
                Console.WriteLine("=====================================");
                Console.ResetColor();

                if (response == null || response.Items == null || !response.Items.Any())
                {
                    Console.WriteLine("\nNo records found.");
                    ConsoleHelper.WaitForKey();
                    return;
                }

                // Render Table Header
                Console.WriteLine("\n" + string.Join(" | ", headers));
                Console.WriteLine(new string('-', headers.Sum(h => h.Length) + (headers.Length * 3)));

                // Render Rows
                foreach (var item in response.Items)
                {
                    Console.WriteLine(string.Join(" | ", rowFormatter(item)));
                }

                var meta = response.Metadata;
                if (meta == null)
                {
                    ConsoleHelper.WaitForKey();
                    return;
                }

                // Render Pagination Controls
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\nPage {meta.currentPage} of {meta.totalPages} (Total Records: {meta.totalCount})");
                Console.ResetColor();

                Console.WriteLine("Press [N]ext, [P]revious, or [Q]uit:");
                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.N && meta.hasNext)
                {
                    currentPage++;
                }
                else if (key == ConsoleKey.P && meta.hasPrevious)
                {
                    currentPage--;
                }
                else if (key == ConsoleKey.Q)
                {
                    return;
                }
            }
        }
    }
}
