namespace ManagedCode.MCPGateway.Tests;

public sealed class McpGatewayInMemoryToolEmbeddingStoreTests
{
    [TUnit.Core.Test]
    public async Task GetAsync_ReturnsEmbeddingsForMatchingLookups()
    {
        var store = new McpGatewayInMemoryToolEmbeddingStore();
        await store.UpsertAsync(
        [
            new McpGatewayToolEmbedding(
                "local:github_search_issues",
                "local",
                "github_search_issues",
                "hash-1",
                "fingerprint-a",
                [1f, 2f, 3f]),
            new McpGatewayToolEmbedding(
                "local:weather_search_forecast",
                "local",
                "weather_search_forecast",
                "hash-2",
                "fingerprint-a",
                [4f, 5f, 6f])
        ]);

        var result = await store.GetAsync(
        [
            new McpGatewayToolEmbeddingLookup("local:weather_search_forecast", "hash-2", "fingerprint-a"),
            new McpGatewayToolEmbeddingLookup("local:missing", "hash-3", "fingerprint-a"),
            new McpGatewayToolEmbeddingLookup("local:github_search_issues", "hash-1", "fingerprint-a")
        ]);

        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result.Any(static item => item.ToolId == "local:github_search_issues")).IsTrue();
        await Assert.That(result.Any(static item => item.ToolId == "local:weather_search_forecast")).IsTrue();
    }

    [TUnit.Core.Test]
    public async Task UpsertAsync_ClonesVectorsOnWriteAndRead()
    {
        var store = new McpGatewayInMemoryToolEmbeddingStore();
        var inputVector = new[] { 1f, 2f, 3f };

        await store.UpsertAsync(
        [
            new McpGatewayToolEmbedding(
                "local:github_search_issues",
                "local",
                "github_search_issues",
                "hash-1",
                "fingerprint-a",
                inputVector)
        ]);

        inputVector[0] = 99f;

        var firstRead = await store.GetAsync(
        [
            new McpGatewayToolEmbeddingLookup("local:github_search_issues", "hash-1", "fingerprint-a")
        ]);
        firstRead[0].Vector[1] = 77f;

        var secondRead = await store.GetAsync(
        [
            new McpGatewayToolEmbeddingLookup("local:github_search_issues", "hash-1", "fingerprint-a")
        ]);

        await Assert.That(secondRead[0].Vector[0]).IsEqualTo(1f);
        await Assert.That(secondRead[0].Vector[1]).IsEqualTo(2f);
        await Assert.That(secondRead[0].Vector[2]).IsEqualTo(3f);
    }

    [TUnit.Core.Test]
    public async Task GetAsync_TreatsToolIdsCaseInsensitivelyAndSupportsFingerprintFallback()
    {
        var store = new McpGatewayInMemoryToolEmbeddingStore();
        await store.UpsertAsync(
        [
            new McpGatewayToolEmbedding(
                "local:github_search_issues",
                "local",
                "github_search_issues",
                "hash-1",
                "fingerprint-a",
                [1f, 2f, 3f])
        ]);

        var fingerprintMatch = await store.GetAsync(
        [
            new McpGatewayToolEmbeddingLookup("LOCAL:GITHUB_SEARCH_ISSUES", "hash-1", "fingerprint-a")
        ]);
        var fingerprintAgnosticMatch = await store.GetAsync(
        [
            new McpGatewayToolEmbeddingLookup("LOCAL:GITHUB_SEARCH_ISSUES", "hash-1")
        ]);

        await Assert.That(fingerprintMatch.Count).IsEqualTo(1);
        await Assert.That(fingerprintAgnosticMatch.Count).IsEqualTo(1);
        await Assert.That(fingerprintAgnosticMatch[0].ToolId).IsEqualTo("local:github_search_issues");
    }
}
