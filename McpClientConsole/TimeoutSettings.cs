namespace McpClientConsole;

internal sealed class TimeoutSettings
{
    public int ConnectionLifetimeInSeconds { get; }
    public int IdleTimeoutInSeconds { get; }
    public int TransportLayerConnectionTimeoutInSeconds { get; }
    public int McpInitializationTimeoutInSeconds { get; }

    internal TimeoutSettings(
        int connectionLifetimeInSeconds, 
        int idleTimeoutInSeconds, 
        int transportLayerConnectionTimeoutInSeconds, 
        int mcpInitializationTimeoutInSeconds)
    {
        this.ConnectionLifetimeInSeconds = connectionLifetimeInSeconds;
        this.IdleTimeoutInSeconds = idleTimeoutInSeconds;
        this.TransportLayerConnectionTimeoutInSeconds = transportLayerConnectionTimeoutInSeconds;
        this.McpInitializationTimeoutInSeconds = mcpInitializationTimeoutInSeconds;
    }
}