using Microsoft.Extensions.Options;

namespace ManagedCode.MCPGateway;

internal sealed class McpGatewayRegistryState(IOptions<McpGatewayOptions> options) : IAsyncDisposable
{
    private readonly McpGatewayRegistrationCollection _registrations = new(options.Value.SourceRegistrations);
    private int _version;
    private int _disposed;
    private int _activeOperations;
    private TaskCompletionSource<object?>? _drainSignal;

    public McpGatewayCatalogSourceSnapshot CreateSnapshot()
    {
        EnterOperation();
        try
        {
            ThrowIfDisposed();
            return new McpGatewayCatalogSourceSnapshot(
                Volatile.Read(ref _version),
                _registrations.Snapshot());
        }
        finally
        {
            ExitOperation();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        Interlocked.Increment(ref _version);
        var drainSignal = EnsureDrainSignal();
        if (Volatile.Read(ref _activeOperations) > 0)
        {
            await drainSignal.Task;
        }

        var registrations = _registrations.Drain();
        foreach (var registration in registrations)
        {
            await registration.DisposeAsync();
        }
    }

    public void Mutate(Action<McpGatewayRegistrationCollection> mutation)
    {
        EnterOperation();
        try
        {
            ThrowIfDisposed();
            mutation(_registrations);
            Interlocked.Increment(ref _version);
        }
        finally
        {
            ExitOperation();
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
    }

    private void EnterOperation()
    {
        while (true)
        {
            ThrowIfDisposed();
            Interlocked.Increment(ref _activeOperations);
            if (Volatile.Read(ref _disposed) == 0)
            {
                return;
            }

            ExitOperation();
        }
    }

    private void ExitOperation()
    {
        if (Interlocked.Decrement(ref _activeOperations) == 0)
        {
            Volatile.Read(ref _drainSignal)?.TrySetResult(null);
        }
    }

    private TaskCompletionSource<object?> EnsureDrainSignal()
    {
        while (true)
        {
            var existing = Volatile.Read(ref _drainSignal);
            if (existing is not null)
            {
                if (Volatile.Read(ref _activeOperations) == 0)
                {
                    existing.TrySetResult(null);
                }

                return existing;
            }

            var created = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (Interlocked.CompareExchange(ref _drainSignal, created, null) is null)
            {
                if (Volatile.Read(ref _activeOperations) == 0)
                {
                    created.TrySetResult(null);
                }

                return created;
            }
        }
    }
}
