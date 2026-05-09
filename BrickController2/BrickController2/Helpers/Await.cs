using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using static BrickController2.Diagnostics.Logs;

namespace BrickController2.Helpers;

internal static class Await
{
    internal static async Task<bool> WaitForStableValueAsync<TValue>(TimeSpan timeout,
        Func<TValue> getValue,
        Func<TValue, TValue, bool> stabilityCheck,
        CancellationToken token = default)
        where TValue : struct
    {
        var interval = TimeSpan.FromMilliseconds(50);
        var stabilityTimeout = TimeSpan.FromMilliseconds(200);

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
            Dump("TimestampStability: TIMEOUT", lastValue);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            // total timeout elapsed — treat as completed
            Dump("TimestampStability: CANCELLED", lastValue);
        }
        return false;
    }
}
