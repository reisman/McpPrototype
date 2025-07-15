using System.Text.Json;

namespace McpServer;

internal static class HttpClientExtensions
{
    internal static async ValueTask<JsonDocument> ReadJsonDocumentAsync(
        this HttpClient client, 
        Uri requestUri,
        CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
    }
    
    internal static async ValueTask<string> ReadAsync(
        this HttpClient client, 
        Uri requestUri,
        CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return content;
    }
}