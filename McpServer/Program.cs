using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
services.AddOpenApi();
services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithPromptsFromAssembly()
    .WithResourcesFromAssembly();

services.AddHttpClient("BomApiClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5033");
    var productInfo = new ProductInfoHeaderValue("bom-tool", "1.0");
    client.DefaultRequestHeaders.UserAgent.Add(productInfo);

    var configurationRoot = new ConfigurationBuilder()
        .AddJsonFile("secrets.json")
        .Build();
    
    var apiKey = configurationRoot["ApiKey"];
    client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
});

var app = builder.Build();
app.MapOpenApi();
app.MapMcp();
app.UseHttpsRedirection();

app.Run();