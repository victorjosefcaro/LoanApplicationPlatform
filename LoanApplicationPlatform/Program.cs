using System.Text.Json;

Console.WriteLine("Fetching data from the API...");

using var client = new HttpClient();
client.BaseAddress = new Uri("https://localhost:7103");

try
{
    var response = await client.GetAsync("/WeatherForecast");
    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync();
    
    var options = new JsonSerializerOptions { WriteIndented = true };
    var parsedJson = JsonSerializer.Deserialize<object>(content);
    var formattedJson = JsonSerializer.Serialize(parsedJson, options);

    Console.WriteLine(formattedJson);
}
catch (Exception ex)
{
    Console.WriteLine($"Error fetching data: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
