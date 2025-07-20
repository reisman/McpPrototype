using System.Net.Http.Headers;

namespace McpServer;

internal interface IConfigurationReader
{
    ProductInfoHeaderValue GetProductInfo();
    string? GetApiKey();
    Uri GetApiEndpoint();
}