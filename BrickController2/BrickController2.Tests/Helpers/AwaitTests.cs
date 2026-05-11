using BrickController2.Helpers;
using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BrickController2.Tests.Helpers;

public class AwaitTests
{
    private static Func<int, int, bool> IsStable => (current, last) => current == last;

    [Fact]
    public async Task WaitForStableValueAsync_ReturnsTrue_WhenValueBecomesStable()
    {
        var callCount = 0;
        var values = new[] { 1, 2, 2, 2, 2, 2, 2, 2 };
        int GetValue() => values[Math.Min(callCount++, values.Length - 1)];

        var result = await Await.WaitForStableValueAsync(GetValue,
            IsStable,
            TimeSpan.FromMilliseconds(500));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task WaitForStableValueAsync_ReturnsFalse_WhenTimeoutExpiresWithUnstableValue()
    {
        var callCount = 0;
        int GetValue() => callCount++; // always changing

        var result = await Await.WaitForStableValueAsync(
            GetValue,
            IsStable,
            TimeSpan.FromMilliseconds(500));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task WaitForStableValueAsync_ReturnsFalse_WhenCancellationTokenCancelled()
    {
        using var cts = new CancellationTokenSource();
        var callCount = 0;
        int GetValue() => callCount++;

        await cts.CancelAsync();

        var result = await Await.WaitForStableValueAsync(
            GetValue,
            IsStable,
            TimeSpan.FromSeconds(1),
            cts.Token);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task WaitForStableValueAsync_ReturnsTrue_WhenValueIsImmediatelyStable()
    {
        int GetValue() => 42;

        var result = await Await.WaitForStableValueAsync(
            GetValue,
            IsStable,
            TimeSpan.FromSeconds(1));

        result.Should().BeTrue();
    }
}