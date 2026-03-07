using ManagedCode.MCPGateway.Abstractions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManagedCode.MCPGateway;

public sealed class McpGateway : IMcpGateway
{
    private readonly McpGatewayRuntime _runtime;

    public McpGateway(
        IServiceProvider serviceProvider,
        IOptions<McpGatewayOptions> options,
        ILogger<McpGateway> logger,
        ILoggerFactory loggerFactory)
        : this(new McpGatewayRuntime(serviceProvider, options, logger, loggerFactory))
    {
    }

    internal McpGateway(McpGatewayRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        _runtime = runtime;
    }

    public Task<McpGatewayIndexBuildResult> BuildIndexAsync(CancellationToken cancellationToken = default)
        => _runtime.BuildIndexAsync(cancellationToken);

    public Task<IReadOnlyList<McpGatewayToolDescriptor>> ListToolsAsync(CancellationToken cancellationToken = default)
        => _runtime.ListToolsAsync(cancellationToken);

    public Task<McpGatewaySearchResult> SearchAsync(
        string? query,
        int? maxResults = null,
        CancellationToken cancellationToken = default)
        => _runtime.SearchAsync(query, maxResults, cancellationToken);

    public Task<McpGatewaySearchResult> SearchAsync(
        McpGatewaySearchRequest request,
        CancellationToken cancellationToken = default)
        => _runtime.SearchAsync(request, cancellationToken);

    public Task<McpGatewayInvokeResult> InvokeAsync(
        McpGatewayInvokeRequest request,
        CancellationToken cancellationToken = default)
        => _runtime.InvokeAsync(request, cancellationToken);

    public IReadOnlyList<AITool> CreateMetaTools(
        string searchToolName = McpGatewayToolSet.DefaultSearchToolName,
        string invokeToolName = McpGatewayToolSet.DefaultInvokeToolName)
        => _runtime.CreateMetaTools(searchToolName, invokeToolName);

    public ValueTask DisposeAsync() => _runtime.DisposeAsync();
}
