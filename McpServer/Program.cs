using McpServer;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

var services = builder.Services;
services.AddLogging();
services.AddSingleton<IConfigurationReader, ConfigurationReader>();
services.AddOpenApi();

services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithPromptsFromAssembly()
    .WithResourcesFromAssembly();

services.AddHttpClient("BomApiClient", (serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfigurationReader>();
    
    var productInfo = configuration.GetProductInfo();
    client.DefaultRequestHeaders.UserAgent.Add(productInfo);
    client.BaseAddress = configuration.GetApiEndpoint();
    
    var apiKey = configuration.GetApiKey();
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
    }
});

var app = builder.Build();
app.UseHttpsRedirection();
app.MapOpenApi();
app.MapMcp();

app.Run();