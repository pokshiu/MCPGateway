using ManagedCode.MCPGateway.Abstractions;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace ManagedCode.MCPGateway;

internal sealed class McpGatewayRegistry(McpGatewayRegistryState state) : IMcpGatewayRegistry, IMcpGatewayCatalogSource, IAsyncDisposable
{
    public void AddTool(string sourceId, AITool tool, string? displayName = null)
        => state.Mutate(registrations => registrations.AddTool(sourceId, tool, displayName));

    public void AddTool(AITool tool, string sourceId = McpGatewayDefaults.DefaultSourceId, string? displayName = null)
        => state.Mutate(registrations => registrations.AddTool(tool, sourceId, displayName));

    public void AddTools(string sourceId, IEnumerable<AITool> tools, string? displayName = null)
        => state.Mutate(registrations => registrations.AddTools(sourceId, tools, displayName));

    public void AddTools(IEnumerable<AITool> tools, string sourceId = McpGatewayDefaults.DefaultSourceId, string? displayName = null)
        => state.Mutate(registrations => registrations.AddTools(tools, sourceId, displayName));

    public void AddHttpServer(
        string sourceId,
        Uri endpoint,
        IReadOnlyDictionary<string, string>? headers = null,
        string? displayName = null)
        => state.Mutate(registrations => registrations.AddHttpServer(sourceId, endpoint, headers, displayName));

    public void AddStdioServer(
        string sourceId,
        string command,
        IReadOnlyList<string>? arguments = null,
        string? workingDirectory = null,
        IReadOnlyDictionary<string, string?>? environmentVariables = null,
        string? displayName = null)
        => state.Mutate(registrations => registrations.AddStdioServer(sourceId, command, arguments, workingDirectory, environmentVariables, displayName));

    public void AddMcpClient(
        string sourceId,
        McpClient client,
        bool disposeClient = false,
        string? displayName = null)
        => state.Mutate(registrations => registrations.AddMcpClient(sourceId, client, disposeClient, displayName));

    public void AddMcpClientFactory(
        string sourceId,
        Func<CancellationToken, ValueTask<McpClient>> clientFactory,
        bool disposeClient = true,
        string? displayName = null)
        => state.Mutate(registrations => registrations.AddMcpClientFactory(sourceId, clientFactory, disposeClient, displayName));

    public McpGatewayCatalogSourceSnapshot CreateSnapshot() => state.CreateSnapshot();

    public ValueTask DisposeAsync() => state.DisposeAsync();
}
