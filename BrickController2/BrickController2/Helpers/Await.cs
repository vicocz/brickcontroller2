using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using static BrickController2.Diagnostics.Logs;

namespace BrickController2.Helpers;

internal static class Await
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(50);
    private static readonly TimeSpan DefaultStabilityTimeout = TimeSpan.FromMilliseconds(200);

    internal static ValueTask<bool> WaitForStableValueAsync<TValue>(Func<TValue> getValue,
        Func<TValue, TValue, bool> stabilityCheck,
        TimeSpan timeout,
        CancellationToken token = default)
        where TValue : struct
        => WaitForStableValueAsync(getValue, stabilityCheck, timeout, DefaultStabilityTimeout, token);

    internal static async ValueTask<bool> WaitForStableValueAsync<TValue>(Func<TValue> getValue,
        Func<TValue, TValue, bool> stabilityCheck,
        TimeSpan timeout,
        TimeSpan stabilityTimeout,
        CancellationToken token = default)
        where TValue : struct
    {
        var interval = DefaultInterval;

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        linkedCts.CancelAfter(timeout);

        var stableSince = Stopwatch.StartNew();
        var lastValue = getValue();

        try
        {
            while (!linkedCts.Token.IsCancellationRequested)
            {
                await Task.Delay(interval, linkedCts.Token);

                var currentValue = getValue();
                if (!stabilityCheck(currentValue, lastValue))
                {
                    lastValue = currentValue;
                    stableSince.Restart();
                }
                else if (stableSince.Elapsed >= stabilityTimeout)
                {
                    return true; // position stable for the required duration
                }
            }
            Dump("Stable Value: TIMEOUT", lastValue);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            // total timeout elapsed — treat as completed
            Dump("Stable Value: CANCELLED", lastValue);
        }
        return false;
    }
}
