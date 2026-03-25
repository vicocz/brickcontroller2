using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement.IO;

internal class DeviceInitializationWaiter
{
    // Time to wait after the LAST port message before assuming sync is complete
    private static readonly TimeSpan BurstTimeout = TimeSpan.FromMilliseconds(500);
    // Absolute maximum time to wait to prevent infinite hanging
    private static readonly TimeSpan AbsoluteTimeout = TimeSpan.FromMilliseconds(4000);

    private readonly TaskCompletionSource<bool> _initializationTcs = new();
    private CancellationTokenSource _debounceCts = new();

    /// <summary>
    /// Waits until the debounce window elapses with no new port events,
    /// or until the absolute timeout is reached.
    /// </summary>
    /// <returns><c>true</c> if initialization completed normally; <c>false</c> if the absolute timeout elapsed.</returns>
    public async Task<bool> WaitAsync(CancellationToken token)
    {
        var absoluteTimeoutTask = Task.Delay(AbsoluteTimeout, token);
        var completedTask = await Task.WhenAny(_initializationTcs.Task, absoluteTimeoutTask);

        if (completedTask == absoluteTimeoutTask)
        {
            return false;
        }

        return await _initializationTcs.Task;
    }

    /// <summary>
    /// Resets the sliding debounce window. Call this each time a port attach event is received.
    /// </summary>
    public void NotifyPortAttached()
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();

        Task.Delay(BurstTimeout, _debounceCts.Token).ContinueWith(t =>
        {
            if (!t.IsCanceled && _initializationTcs is { Task.IsCompleted: false })
            {
                _initializationTcs.TrySetResult(true);
            }
        });
    }
}
