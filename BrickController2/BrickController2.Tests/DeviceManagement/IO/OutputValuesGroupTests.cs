using BrickController2.DeviceManagement.IO;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.IO;

public class OutputValuesGroupTests
{
    [Theory]
    [InlineData(3)]
    [InlineData(7)]
    public void Initialize_NoChangeYet_ReturnsTrueAndAllDefaultValues(int channelCount)
    {
        // Arrange
        var group = new OutputValuesGroup<float>(channelCount);
        // Act
        group.Initialize();

        // Assert
        var result = group.TryGetValues(out var values);
        result.Should().BeTrue();
        values.Length.Should().Be(channelCount);
        values.ToArray().Should().AllBeEquivalentTo(0.0f);
    }

    [Fact]
    public void Initialize_NoCommit_ChangeIsReportedFiveTimesOnly()
    {
        // Arrange
        var group = new OutputValuesGroup<byte>(1);
        // Act
        group.Initialize();

        // Assert
        for (int i = 0; i < 5; i++)
        {
            group.TryGetValues(out var values).Should().BeTrue();
            values.ToArray().Should().AllBeEquivalentTo(0);
        }
        group.TryGetValues(out var lastValues).Should().BeFalse();
        lastValues.ToArray().Should().AllBeEquivalentTo(0);
    }

    [Fact]
    public void Initialize_SingleChangeToOne_ChangedChannelValueReturned()
    {
        // Arrange
        var group = new OutputValuesGroup<byte>(1);
        // Act
        group.Initialize();
        group.SetOutput(0, 1);

        var result = group.TryGetValues(out var values);

        // Assert
        result.Should().BeTrue();
        values.ToArray().Should().BeEquivalentTo([1]);
    }

    [Fact]
    public void Clear_AnyChange_ReturnsFalseAndAllDefaultValues()
    {
        // Arrange
        var group = new OutputValuesGroup<Half>(5);
        group.SetOutput(3, Half.Pi);
        // Act
        group.Clear();

        // Assert
        var result = group.TryGetValues(out var values);
        result.Should().BeFalse();
        values.Length.Should().Be(5);
        values.ToArray().Should().AllBeEquivalentTo(Half.Zero);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(7)]
    public void TryGetValues_NoChangeYet_NoChangeReported(int channelCount)
    {
        // Arrange
        var group = new OutputValuesGroup<int>(channelCount);

        // Act
        var result = group.TryGetValues(out var values);

        // Assert
        result.Should().BeFalse();
        values.Length.Should().Be(channelCount);
        values.ToArray().Should().AllBeEquivalentTo(0);
    }

    [Fact]
    public void TryGetChanges_NoChangeYet_NoChangeReported()
    {
        // Arrange
        var group = new OutputValuesGroup<int>(5);

        // Act
        var result = group.TryGetChanges(out var changes);

        // Assert
        result.Should().BeFalse();
        changes.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void TryGetValues_SingleChange_ChangedChannelValueReturned()
    {
        // Arrange
        var group = new OutputValuesGroup<float>(2);
        group.SetOutput(1, 3.14f);

        // Act
        var result = group.TryGetValues(out var values);

        // Assert
        result.Should().BeTrue();
        values.ToArray().Should().BeEquivalentTo([0, 3.14f]);
    }

    [Fact]
    public void TryGetChanges_SingleChange_OnlyChangedChannelValueReturned()
    {
        // Arrange
        var group = new OutputValuesGroup<float>(2);
        group.SetOutput(1, 3.14f);

        // Act
        var result = group.TryGetChanges(out var changes);

        // Assert
        result.Should().BeTrue();
        changes.Should().NotBeNull().And.ContainSingle();
        changes.Should().BeEquivalentTo([new KeyValuePair<int, float>(1, 3.14f)]);
    }

    [Fact]
    public void Commit_ExistingChange_NoChangeIsReportedThen()
    {
        // Arrange
        var group = new OutputValuesGroup<short>(2);
        group.SetOutput(0, 7);
        group.TryGetChanges(out var changedValues).Should().BeTrue();
        changedValues.Should().BeEquivalentTo([new KeyValuePair<int, short>(0, 7)]);

        // Act
        group.Commit();

        // Assert
        group.TryGetChanges(out var values).Should().BeFalse();
        values.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void SetOutput_BetweenTryGetValuesAndCommit_IsNotLost()
    {
        var group = new OutputValuesGroup<int>(1);
        group.Initialize();

        // consumer takes the first snapshot and starts "sending"
        group.SetOutput(0, 1);
        Assert.True(group.TryGetValues(out var first));
        Assert.Equal(1, first[0]);

        // joystick event arrives while the send is in flight
        group.SetOutput(0, 2);

        // send of the first snapshot completes
        group.Commit();

        // the newer value must still be delivered
        Assert.True(group.TryGetValues(out var second));
        Assert.Equal(2, second[0]);
    }

    [Fact]
    public void Commit_AfterSuccessfulSend_StopsFurtherSending()
    {
        var group = new OutputValuesGroup<int>(1);
        group.Initialize();

        group.SetOutput(0, 1);
        Assert.True(group.TryGetValues(out _));
        group.Commit();

        Assert.False(group.TryGetValues(out _));
    }

    [Fact]
    public void FailedSend_IsRetriedOnlyLimitedTimes()
    {
        const int maxSendAttempts = 5; // mirrors the private MAX_SEND_ATTEMPTS

        var group = new OutputValuesGroup<int>(1);
        group.Initialize();
        group.SetOutput(0, 1);

        // consumer never calls Commit(), i.e. every send fails
        var attempts = 0;
        while (group.TryGetValues(out _) && attempts < 100)
        {
            attempts++;
        }

        Assert.Equal(maxSendAttempts, attempts);
    }

    /// <summary>
    /// Commit() copies _values to _commitedOutputValues without holding _outputLock,
    /// while Clear() resets both arrays under the lock. If both run concurrently,
    /// Commit() can write stale values into the committed state AFTER Clear() has
    /// already reset it.
    ///
    /// This is a stress test (probabilistic, needs more than one CPU core):
    /// a large channel count widens the race window.
    /// </summary>
    [Fact]
    public async Task Commit_RunningConcurrentlyWithClear_MustNotLeaveStaleValuesInCommittedState()
    {
        const int channels = 50_000;
        const int iterations = 500;

        var staleCount = 0;

        for (var i = 0; i < iterations; i++)
        {
            var group = new OutputValuesGroup<int>(channels);

            // fill outputs and take a snapshot, so _values contains only 1s
            for (var channel = 0; channel < channels; channel++)
            {
                group.SetOutput(channel, 1);
            }
            Assert.True(group.TryGetValues(out _));

            // start Commit() and Clear() at the same moment
            using var barrier = new Barrier(2);

            var commit = Task.Run(() =>
            {
                barrier.SignalAndWait();
                group.Commit();
            }, TestContext.Current.CancellationToken);

            var clear = Task.Run(() =>
            {
                barrier.SignalAndWait();
                group.Clear();
            }, TestContext.Current.CancellationToken);

            await Task.WhenAll(commit, clear);

            // Whatever the order was, the final state must be "everything cleared":
            // outputs are all 0 and committed values must be all 0 as well.
            // Toggle one output so that sending is enabled again (attempts left),
            // while the values stay all 0.
            group.SetOutput(0, 1);
            group.SetOutput(0, 0);

            // If committed values are all 0, there is nothing to send.
            // If stale 1s are left in the committed state, the snapshot differs from it.
            if (group.TryGetValues(out _))
            {
                staleCount++;
            }
        }

        Assert.True(staleCount == 0,
            $"Commit() left stale values in the committed state after Clear() in {staleCount} of {iterations} runs.");
    }

    [Fact]
    public async Task ProducersAndSingleConsumer_FinalValuesAreDelivered()
    {
        const int channels = 4;
        const int updates = 5000;

        var group = new OutputValuesGroup<int>(channels);
        group.Initialize();

        // every 3rd send fails, so the retry logic is exercised as well
        var device = new FakeDevice(channels, failEvery: 3);

        using var cts = new CancellationTokenSource();
        var consumer = Task.Run(() => RunConsumerLoop(group, device, cts.Token), TestContext.Current.CancellationToken);

        // joystick events, one producer per channel
        var producers = Enumerable.Range(0, channels)
            .Select(channel => Task.Run(() =>
            {
                for (var value = 1; value <= updates; value++)
                {
                    group.SetOutput(channel, value);
                }
            }, TestContext.Current.CancellationToken))
            .ToArray();

        await Task.WhenAll(producers);

        var delivered = SpinWait.SpinUntil(
            () => device.GetLastFrame().All(v => v == updates),
            TimeSpan.FromSeconds(5));

        cts.Cancel();
        await consumer;

        Assert.True(delivered, $"Last frame on device: [{string.Join(", ", device.GetLastFrame())}]");
    }

    /// <summary>
    /// Mirrors the loop in BuWizz2Device.ProcessOutputsAsync (without output level handling).
    /// </summary>
    private static void RunConsumerLoop(OutputValuesGroup<int> group, FakeDevice device, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (group.TryGetValues(out var values))
            {
                // copy the span, like the real code does when it builds the BLE buffer
                if (device.TrySend(values.ToArray()))
                {
                    group.Commit();
                }
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    private sealed class FakeDevice(int channels, int failEvery)
    {
        private readonly object _lock = new();
        private int[] _lastFrame = new int[channels];
        private int _sendCount;

        public bool TrySend(int[] frame)
        {
            lock (_lock)
            {
                _sendCount++;
                if (_sendCount % failEvery == 0)
                {
                    return false; // simulated BLE write failure
                }

                _lastFrame = frame;
                return true;
            }
        }

        public int[] GetLastFrame()
        {
            lock (_lock)
            {
                return [.. _lastFrame];
            }
        }
    }
}