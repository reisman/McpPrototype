using McpServer;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

var services = builder.Services;
services.AddLogging();
services.AddSingleton<IConfigurationReader, ConfigurationReader>();
services.AddOpenApi();
services.AddHttpClient("BomApiClient", HttpClientConfiguration.ConfigureClient);
services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithPromptsFromAssembly()
    .WithResourcesFromAssembly();

var app = builder.Build();
app.UseHttpsRedirection();
app.MapOpenApi();
app.MapMcp();
app.Run();