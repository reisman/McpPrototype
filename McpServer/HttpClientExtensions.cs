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
    
    internal static ValueTask<HttpResponseMessage> Delete(
        this HttpClient client, 
        Uri requestUri, 
        CancellationToken cancellationToken)
    {
        var pipeline = CreatePipeline();
        return pipeline.ExecuteAsync(async token => await client.DeleteAsync(requestUri, token), cancellationToken);
    }
    
    internal static ValueTask<HttpResponseMessage> Post<T>(
        this HttpClient client, 
        Uri requestUri, 
        T contentObj, 
        CancellationToken cancellationToken)
    {
        var content = Serialize(contentObj);
        var pipeline = CreatePipeline();
        return pipeline.ExecuteAsync(async token => await client.PostAsync(requestUri, content, token), cancellationToken);
    }
    
    internal static ValueTask<HttpResponseMessage> Patch<T>(
        this HttpClient client, 
        Uri requestUri, 
        T contentObj, 
        CancellationToken cancellationToken)
    {
        var content = Serialize(contentObj);
        var pipeline = CreatePipeline();
        return pipeline.ExecuteAsync(async token => await client.PatchAsync(requestUri, content, token), cancellationToken);
    }

    private static StringContent Serialize<T>(T obj)
    {
        var partString = JsonSerializer.Serialize(obj);
        return new StringContent(partString, System.Text.Encoding.UTF8, "application/json");
    }
    
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
}