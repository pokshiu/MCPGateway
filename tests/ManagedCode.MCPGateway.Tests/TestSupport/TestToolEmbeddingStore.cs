using ManagedCode.MCPGateway.Abstractions;

namespace ManagedCode.MCPGateway.Tests;

internal sealed class TestToolEmbeddingStore : IMcpGatewayToolEmbeddingStore
{
    private readonly Dictionary<McpGatewayToolEmbeddingLookup, McpGatewayToolEmbedding> _embeddings = [];

    public List<IReadOnlyList<McpGatewayToolEmbeddingLookup>> GetCalls { get; } = [];

    public List<IReadOnlyList<McpGatewayToolEmbedding>> UpsertCalls { get; } = [];

    public Task<IReadOnlyList<McpGatewayToolEmbedding>> GetAsync(
        IReadOnlyList<McpGatewayToolEmbeddingLookup> lookups,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        GetCalls.Add(lookups.ToList());

        var matches = lookups
            .Where(_embeddings.ContainsKey)
            .Select(lookup => Clone(_embeddings[lookup]))
            .ToList();

        return Task.FromResult<IReadOnlyList<McpGatewayToolEmbedding>>(matches);
    }

    public Task UpsertAsync(
        IReadOnlyList<McpGatewayToolEmbedding> embeddings,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var clonedBatch = embeddings
            .Select(Clone)
            .ToList();
        UpsertCalls.Add(clonedBatch);

        foreach (var embedding in clonedBatch)
        {
            _embeddings[new McpGatewayToolEmbeddingLookup(embedding.ToolId, embedding.DocumentHash)] = embedding;
        }

        return Task.CompletedTask;
    }

    public void Remove(string toolId)
    {
        var keys = _embeddings.Keys
            .Where(lookup => string.Equals(lookup.ToolId, toolId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keys)
        {
            _embeddings.Remove(key);
        }
    }

    private static McpGatewayToolEmbedding Clone(McpGatewayToolEmbedding embedding)
        => embedding with
        {
            Vector = [.. embedding.Vector]
        };
}
