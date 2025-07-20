using System.Net.Http.Headers;

namespace McpServer;

internal sealed class ConfigurationReader(IConfiguration configuration, ILogger<ConfigurationReader> logger) : IConfigurationReader
{
    public ProductInfoHeaderValue GetProductInfo()
    {
        var productName = configuration["ProductInfo:Name"];
        if (string.IsNullOrWhiteSpace(productName))
        {
            const string message = "ProductInfo:Name is not configured. Please set it in the configuration file.";
            logger.LogError(message);
            throw new InvalidOperationException(message);
        }
    
        var productVersion = configuration["ProductInfo:Version"];
        if (string.IsNullOrWhiteSpace(productVersion))
        {
            const string message = "ProductInfo:Version is not configured. Please set it in the configuration file.";
            logger.LogError(message);
            throw new InvalidOperationException(message);
        }
    
        return new ProductInfoHeaderValue(productName, productVersion);
    }

    public string? GetApiKey()
    {
        return configuration["ApiKey"];
    }

    public Uri GetApiEndpoint()
    {
        var apiEndpoint = configuration["Endpoints:ApiEndpoint"];
        if (string.IsNullOrWhiteSpace(apiEndpoint))
        {
            var message = "Endpoints:ApiEndpoint is not configured. Please set it in the configuration file.";
            logger.LogError(message);
            throw new InvalidOperationException(message);
        }
        
        return new Uri(apiEndpoint);
    }
}