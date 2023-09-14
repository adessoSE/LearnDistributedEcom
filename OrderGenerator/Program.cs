// See https://aka.ms/new-console-template for more information
using System.Net.Http.Json;

using OrderGenerator;

using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var client = new HttpClient();

Task.Run(async () =>
{
    while(true)
    {
        var order = Generator.CreateOrder();

        try
        {
            await client.PostAsJsonAsync("http://localhost:5088/Orders", order);
        }
        catch (Exception)
        {
            Log.Error("API Error");
            await Task.Delay(TimeSpan.FromSeconds(5));
        }        

        await Task.Delay(TimeSpan.FromSeconds(Random.Shared.NextDouble()));
    }
});

Console.ReadKey();
