using System.Collections.Concurrent;
using ManagedCode.MCPGateway.Abstractions;

namespace ManagedCode.MCPGateway;

public sealed class McpGatewayInMemoryToolEmbeddingStore : IMcpGatewayToolEmbeddingStore
{
    private readonly ConcurrentDictionary<McpGatewayToolEmbeddingLookup, McpGatewayToolEmbedding> _embeddings = new();

    public Task<IReadOnlyList<McpGatewayToolEmbedding>> GetAsync(
        IReadOnlyList<McpGatewayToolEmbeddingLookup> lookups,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var results = lookups
            .Where(_embeddings.ContainsKey)
            .Select(lookup => Clone(_embeddings[lookup]))
            .ToList();

        return Task.FromResult<IReadOnlyList<McpGatewayToolEmbedding>>(results);
    }

    public Task UpsertAsync(
        IReadOnlyList<McpGatewayToolEmbedding> embeddings,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var embedding in embeddings)
        {
            var clone = Clone(embedding);
            _embeddings[new McpGatewayToolEmbeddingLookup(clone.ToolId, clone.DocumentHash)] = clone;
        }

        return Task.CompletedTask;
    }

    private static McpGatewayToolEmbedding Clone(McpGatewayToolEmbedding embedding)
        => embedding with
        {
            Vector = [.. embedding.Vector]
        };
}
