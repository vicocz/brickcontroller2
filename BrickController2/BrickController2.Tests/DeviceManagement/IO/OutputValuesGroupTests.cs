using BrickController2.DeviceManagement.IO;
using FluentAssertions;
using System;
using System.Collections.Generic;
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
    public void AccumulateOutput_ZeroValue_ReturnsFalseAndNoChangeReported()
    {
        // Arrange
        var group = new OutputValuesGroup<float>(3);

        // Act
        var result = group.AccumulateOutput(1, 0.0f);

        // Assert
        result.Should().BeFalse();
        group.TryGetValues(out _).Should().BeFalse();
    }

    [Fact]
    public void AccumulateOutput_NonZeroValue_ReturnsTrueAndChangeReported()
    {
        // Arrange
        var group = new OutputValuesGroup<float>(3);

        // Act
        var result = group.AccumulateOutput(1, 2.5f);

        // Assert
        result.Should().BeTrue();
        group.TryGetValues(out var values).Should().BeTrue();
        values[1].Should().Be(2.5f);
    }

    [Fact]
    public void AccumulateOutput_NegativeValue_AccumulatesNegatively()
    {
        // Arrange
        var group = new OutputValuesGroup<int>(2);
        group.AccumulateOutput(1, 10);
        group.TryGetValues(out _);
        group.Commit();

        // Act
        group.AccumulateOutput(1, -4);

        // Assert
        group.TryGetValues(out var values).Should().BeTrue();
        values[1].Should().Be(6);
    }

    [Fact]
    public void AccumulateOutput_AfterSetOutput_AddsOnTop()
    {
        // Arrange
        var group = new OutputValuesGroup<float>(2);
        group.SetOutput(0, 1.0f);

        // Act
        group.AccumulateOutput(0, 2.0f);

        // Assert
        group.TryGetValues(out var values).Should().BeTrue();
        values[0].Should().Be(3.0f);
    }

    [Fact]
    public void AccumulateOutput_NonZeroValue_SetsMaxSendAttempts()
    {
        // Arrange
        var group = new OutputValuesGroup<int>(1);
        group.AccumulateOutput(0, 1);

        // Assert — change must be reported one time, then stop
        group.TryGetValues(out _).Should().BeTrue($"attempt 1 should return true");
        group.Commit();
        group.TryGetValues(out _).Should().BeFalse("no more send attempts should remain");
    }
}
