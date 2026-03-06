using System.Collections.Concurrent;
using ManagedCode.MCPGateway.Abstractions;

namespace ManagedCode.MCPGateway;

public sealed class McpGatewayInMemoryToolEmbeddingStore : IMcpGatewayToolEmbeddingStore
{
    private readonly ConcurrentDictionary<StoreKey, McpGatewayToolEmbedding> _embeddings = new();

    public Task<IReadOnlyList<McpGatewayToolEmbedding>> GetAsync(
        IReadOnlyList<McpGatewayToolEmbeddingLookup> lookups,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var results = new List<McpGatewayToolEmbedding>(lookups.Count);
        foreach (var lookup in lookups)
        {
            if (TryGetEmbedding(lookup, out var embedding))
            {
                results.Add(Clone(embedding));
            }
        }

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
            _embeddings[StoreKey.FromEmbedding(clone)] = clone;
        }

        return Task.CompletedTask;
    }

    private bool TryGetEmbedding(
        McpGatewayToolEmbeddingLookup lookup,
        out McpGatewayToolEmbedding embedding)
    {
        var storeKey = StoreKey.FromLookup(lookup);
        if (lookup.EmbeddingGeneratorFingerprint is not null)
        {
            return _embeddings.TryGetValue(storeKey, out embedding!);
        }

        foreach (var pair in _embeddings)
        {
            if (pair.Key.Matches(storeKey))
            {
                embedding = pair.Value;
                return true;
            }
        }

        embedding = default!;
        return false;
    }

    private static McpGatewayToolEmbedding Clone(McpGatewayToolEmbedding embedding)
        => embedding with
        {
            Vector = [.. embedding.Vector]
        };

    private static string NormalizeToolId(string toolId) => toolId.ToUpperInvariant();

    private readonly record struct StoreKey(
        string NormalizedToolId,
        string DocumentHash,
        string? EmbeddingGeneratorFingerprint)
    {
        public static StoreKey FromLookup(McpGatewayToolEmbeddingLookup lookup)
            => new(
                NormalizeToolId(lookup.ToolId),
                lookup.DocumentHash,
                lookup.EmbeddingGeneratorFingerprint);

        public static StoreKey FromEmbedding(McpGatewayToolEmbedding embedding)
            => new(
                NormalizeToolId(embedding.ToolId),
                embedding.DocumentHash,
                embedding.EmbeddingGeneratorFingerprint);

        public bool Matches(StoreKey other)
            => string.Equals(NormalizedToolId, other.NormalizedToolId, StringComparison.Ordinal)
                && string.Equals(DocumentHash, other.DocumentHash, StringComparison.Ordinal)
                && (other.EmbeddingGeneratorFingerprint is null
                    || string.Equals(
                        EmbeddingGeneratorFingerprint,
                        other.EmbeddingGeneratorFingerprint,
                        StringComparison.Ordinal));
    }
}
