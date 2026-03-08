using Microsoft.Extensions.AI;

namespace ManagedCode.MCPGateway.Tests;

internal sealed class TestChatClient(TestChatClientOptions? options = null) : IChatClient
{
    private readonly TestChatClientOptions _options = options ?? new();

    public List<string> Calls { get; } = [];

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query = messages.LastOrDefault(static message => message.Role == ChatRole.User)?.Text ?? string.Empty;
        Calls.Add(query);

        if (_options.ThrowOnInput?.Invoke(query) == true)
        {
            throw new InvalidOperationException("Query normalization failed for a test input.");
        }

        var rewrittenQuery = _options.RewriteQuery?.Invoke(query) ?? query;
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, rewrittenQuery)));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = await GetResponseAsync(messages, options, cancellationToken);
        yield break;
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        return null;
    }

    public void Dispose()
    {
    }
}

internal sealed class TestChatClientOptions
{
    public Func<string, string>? RewriteQuery { get; init; }

    public Func<string, bool>? ThrowOnInput { get; init; }
}
