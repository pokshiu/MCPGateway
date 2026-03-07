using ManagedCode.MCPGateway.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ManagedCode.MCPGateway;

public static class McpGatewayServiceCollectionExtensions
{
    public static IServiceCollection AddManagedCodeMcpGateway(
        this IServiceCollection services,
        Action<McpGatewayOptions>? configure = null)
    {
        services.AddOptions<McpGatewayOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<McpGatewayRegistryState>();
        services.TryAddSingleton<IMcpGateway, McpGateway>();
        services.TryAddSingleton<IMcpGatewayCatalogSource, McpGatewayRegistry>();
        services.TryAddSingleton<IMcpGatewayRegistry, McpGatewayRegistry>();
        services.TryAddSingleton<McpGatewayToolSet>();

        return services;
    }
}
