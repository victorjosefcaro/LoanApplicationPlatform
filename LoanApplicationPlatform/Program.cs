using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

var httpClient = new HttpClient
{
    // Make sure this matches your API port from launchSettings.json (HTTPS)
    BaseAddress = new Uri("https://localhost:7103") 
};

string? jwtToken = null;

while (true)
{
    Console.Clear();
    Console.WriteLine("=== Loan Application Platform ===");
    if (!string.IsNullOrEmpty(jwtToken))
    {
        Console.WriteLine("[ Status: Logged In ]");
    }
    else
    {
        Console.WriteLine("[ Status: NOT Logged In ]");
    }

    Console.WriteLine("\n1. Login");
    Console.WriteLine("2. Register (Applicant)");
    Console.WriteLine("3. Test API (Requires Login)");
    Console.WriteLine("4. Logout");
    Console.WriteLine("5. Exit");
    Console.Write("\nSelect an option: ");
    
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await LoginAsync();
            break;
        case "2":
            await RegisterAsync();
            break;
        case "3":
            await TestApiAsync();
            break;
        case "4":
            jwtToken = null;
            httpClient.DefaultRequestHeaders.Authorization = null;
            Console.WriteLine("\nLogged out successfully. Press any key...");
            Console.ReadKey();
            break;
        case "5":
            return;
        default:
            Console.WriteLine("\nInvalid option. Press any key...");
            Console.ReadKey();
            break;
    }
}

async Task LoginAsync()
{
    Console.Write("Username: ");
    var username = Console.ReadLine();
    Console.Write("Password: ");
    var password = Console.ReadLine();

    var response = await httpClient.PostAsJsonAsync("/api/authentication/authenticate", new { Username = username, Password = password });

    if (response.IsSuccessStatusCode)
    {
        jwtToken = await response.Content.ReadAsStringAsync();
        // Remove quotes from the JSON string response
        jwtToken = jwtToken.Trim('"');
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        Console.WriteLine("\nLogin successful! Token saved. Press any key...");
    }
    else
    {
        Console.WriteLine($"\nLogin failed: {response.StatusCode}. Press any key...");
    }
    Console.ReadKey();
}

async Task RegisterAsync()
{
    Console.Write("New Username: ");
    var username = Console.ReadLine();
    Console.Write("New Password: ");
    var password = Console.ReadLine();

    var response = await httpClient.PostAsJsonAsync("/api/authentication/register", new { Username = username, Password = password });

    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("\nRegistration successful! You can now login. Press any key...");
    }
    else
    {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"\nRegistration failed: {response.StatusCode} - {error}. Press any key...");
    }
    Console.ReadKey();
}

async Task TestApiAsync()
{
    Console.WriteLine("\nFetching users from API...");
    var response = await httpClient.GetAsync("/api/test/users");
    
    if (response.IsSuccessStatusCode)
    {
        var content = await response.Content.ReadAsStringAsync();
        var parsedJson = JsonSerializer.Deserialize<object>(content);
        var formattedJson = JsonSerializer.Serialize(parsedJson, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine("\nAPI Response:\n" + formattedJson);
    }
    else
    {
        Console.WriteLine($"\nAPI call failed! Status: {response.StatusCode}");
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            Console.WriteLine("Hint: You are not authorized. Try logging in first.");
        }
    }
    Console.WriteLine("\nPress any key to return...");
    Console.ReadKey();
}
