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
                [1f, 2f, 3f]),
            new McpGatewayToolEmbedding(
                "local:weather_search_forecast",
                "local",
                "weather_search_forecast",
                "hash-2",
                [4f, 5f, 6f])
        ]);

        var result = await store.GetAsync(
        [
            new McpGatewayToolEmbeddingLookup("local:weather_search_forecast", "hash-2"),
            new McpGatewayToolEmbeddingLookup("local:missing", "hash-3"),
            new McpGatewayToolEmbeddingLookup("local:github_search_issues", "hash-1")
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
                inputVector)
        ]);

        inputVector[0] = 99f;

        var firstRead = await store.GetAsync(
        [
            new McpGatewayToolEmbeddingLookup("local:github_search_issues", "hash-1")
        ]);
        firstRead[0].Vector[1] = 77f;

        var secondRead = await store.GetAsync(
        [
            new McpGatewayToolEmbeddingLookup("local:github_search_issues", "hash-1")
        ]);

        await Assert.That(secondRead[0].Vector[0]).IsEqualTo(1f);
        await Assert.That(secondRead[0].Vector[1]).IsEqualTo(2f);
        await Assert.That(secondRead[0].Vector[2]).IsEqualTo(3f);
    }
}
