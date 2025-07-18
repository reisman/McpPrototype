using System.Text.Json;
using Polly;
using Polly.Retry;

namespace McpServer;

internal static class HttpClientExtensions
{
    #region fields
    
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    #endregion
    
    internal static async ValueTask<T?> Get<T>(
        this HttpClient client, 
        Uri requestUri, 
        CancellationToken cancellationToken)
    {
        var response = await CreatePipeline().ExecuteAsync(async token => await client.GetAsync(requestUri, token), cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var jsonDocument =  await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
        return jsonDocument.Deserialize<T>(SerializerOptions);
    }
    
    internal static async ValueTask<string> GetString(
        this HttpClient client, 
        Uri requestUri,
        CancellationToken cancellationToken)
    {
        var response = await CreatePipeline().ExecuteAsync(async token => await client.GetAsync(requestUri, token), cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
    
    internal static async ValueTask Delete(
        this HttpClient client, 
        Uri requestUri, 
        CancellationToken cancellationToken)
    {
        var pipeline = CreatePipeline();
        var response = await pipeline.ExecuteAsync(async token => await client.DeleteAsync(requestUri, token), cancellationToken);
        response.EnsureSuccessStatusCode();
    }
    
    internal static async ValueTask Post<T>(
        this HttpClient client, 
        Uri requestUri, 
        T contentObj, 
        CancellationToken cancellationToken)
    {
        var content = Serialize(contentObj);
        var pipeline = CreatePipeline();
        var response = await pipeline.ExecuteAsync(async token => await client.PostAsync(requestUri, content, token), cancellationToken);
        response.EnsureSuccessStatusCode();
    }
    
    internal static async ValueTask Patch<T>(
        this HttpClient client, 
        Uri requestUri, 
        T contentObj, 
        CancellationToken cancellationToken)
    {
        var content = Serialize(contentObj);
        var pipeline = CreatePipeline();
        var response = await pipeline.ExecuteAsync(async token => await client.PatchAsync(requestUri, content, token), cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    #region serialize
    
    private static StringContent Serialize<T>(T obj)
    {
        var partString = JsonSerializer.Serialize(obj);
        return new StringContent(partString, System.Text.Encoding.UTF8, "application/json");
    }
    
    #endregion
    
    #region pipeline
    
    private static ResiliencePipeline CreatePipeline()
    {
        var retryStrategy = new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(2),
            BackoffType = DelayBackoffType.Exponential
        };
        
        var timeout = TimeSpan.FromSeconds(10);
        
        return new ResiliencePipelineBuilder()
            .AddRetry(retryStrategy)
            .AddTimeout(timeout)
            .Build();
    }
    
    #endregion
}