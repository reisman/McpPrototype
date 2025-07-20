namespace McpServer;

internal static class HttpClientConfiguration
{
    internal static void ConfigureClient(IServiceProvider serviceProvider, HttpClient client)
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
    }
}