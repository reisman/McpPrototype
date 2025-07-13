using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace McpClientConsole;

internal sealed class McpConnection : IAsyncDisposable
{
    #region field
    
    private readonly IMcpClient client;
    private readonly HttpClient httpClient;
    private readonly SseClientTransport transport;
    private readonly ILoggerFactory loggerFactory;
   
    #endregion
    
    #region constructor / dispose
    
    private McpConnection(
        IMcpClient client, 
        HttpClient httpClient, 
        SseClientTransport transport, 
        ILoggerFactory loggerFactory)
    {
        this.client = client;
        this.httpClient = httpClient;
        this.transport = transport;
        this.loggerFactory = loggerFactory;
    }

    public async ValueTask DisposeAsync()
    { 
        await this.client.DisposeAsync();
        await this.transport.DisposeAsync();
        this.httpClient.Dispose();
        this.loggerFactory.Dispose();
    }

    #endregion
    
    #region open
    
    internal static async ValueTask<McpConnection> CreateAndOpen(
        string name, 
        Uri serverUrl,
        TimeoutSettings timeoutSettings,
        Func<ILoggerFactory> loggerFactoryFunc)
    {
        var loggerFactory = loggerFactoryFunc();
        var httpClient = CreateHttpClient(timeoutSettings);
        var transport = CreateTransport(name, serverUrl, httpClient, loggerFactory, timeoutSettings.TransportLayerConnectionTimeoutInSeconds);
        var mcpClient = await CreateMcpClient(transport, timeoutSettings, loggerFactory, CancellationToken.None);
        return new McpConnection(mcpClient, httpClient, transport, loggerFactory);
    }

    private static HttpClient CreateHttpClient(TimeoutSettings timeoutSettings)
    {
        var pooledConnectionLifetime = TimeSpan.FromSeconds(timeoutSettings.ConnectionLifetimeInSeconds);
        var pooledConnectionIdleTimeout = TimeSpan.FromSeconds(timeoutSettings.IdleTimeoutInSeconds);
        
        var sharedHandler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = pooledConnectionLifetime,
            PooledConnectionIdleTimeout = pooledConnectionIdleTimeout
        };

        return new HttpClient(sharedHandler);
    }
    
    private static SseClientTransport CreateTransport(
        string name, 
        Uri serverUrl, 
        HttpClient httpClient,
        ILoggerFactory loggerFactory,
        int connectionTimeoutInSeconds)
    {
        var transportOptions = new SseClientTransportOptions
        {
            Endpoint = serverUrl,
            Name = name,
            ConnectionTimeout = TimeSpan.FromSeconds(connectionTimeoutInSeconds)
        };
        
        return new SseClientTransport(transportOptions, httpClient, loggerFactory);
    }
    
    private static async ValueTask<IMcpClient> CreateMcpClient(
        IClientTransport transport, 
        TimeoutSettings timeoutSettings,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var options = new McpClientOptions
        {
            InitializationTimeout = TimeSpan.FromSeconds(timeoutSettings.McpInitializationTimeoutInSeconds)
        };

        return await McpClientFactory.CreateAsync(transport, options, loggerFactory, cancellationToken);
    }
    
    #endregion
    
    #region tools

    public async ValueTask<IReadOnlyCollection<string>> GetTools()
    {
        var tools = await this.client.ListToolsAsync();
        return tools
            .Select(tool => tool.Name)
            .ToList();
    }

    public ValueTask<CallToolResult> ExecuteTool(string toolName, IReadOnlyDictionary<string, object?> parameters)
    {
        return this.client.CallToolAsync(toolName, parameters);
    }
    
    #endregion
}