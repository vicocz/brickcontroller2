using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.Common;

public abstract class AsyncDisposableBase : IDisposable, IAsyncDisposable
{
    // 0 means false, 1 means true. We use an int for Interlocked thread-safety.
    private int _isDisposed = 0;

    // Public property to check if the object is already dead
    public bool IsDisposed => Volatile.Read(ref _isDisposed) == 1;

    public void Dispose()
    {
        // Interlocked.Exchange atomically sets the value to 1 and returns the original value.
        // If it was already 1, we know another thread beat us to it, so we do nothing.
        if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
        {
            await DisposeAsyncCore().ConfigureAwait(false);

            // Clean up unmanaged resources synchronously
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }
    }

    protected abstract void Dispose(bool disposing);

    protected abstract ValueTask DisposeAsyncCore();
}
