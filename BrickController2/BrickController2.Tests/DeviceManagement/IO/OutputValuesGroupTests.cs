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

    [Theory]
    [InlineData(0b00000001)]
    [InlineData(0b00000010)]
    [InlineData(0b10000000)]
    [InlineData(0b11111111)]
    public void SetFlag_SetFlagsCorrectly(ulong bitMask)
    {
        Half bitMask_Half = Half.CreateChecked(bitMask);

        // Arrange
        var group = new OutputValuesGroup<Half>(3);

        // Act
        group.SetFlag(1, bitMask_Half, true);

        // Assert
        var result = group.TryGetValues(out var values);
        result.Should().BeTrue();
        values.Length.Should().Be(3);
        values[0].Should().BeEquivalentTo(Half.Zero);
        values[1].Should().BeEquivalentTo(bitMask_Half);
        values[2].Should().BeEquivalentTo(Half.Zero);
    }

    [Theory]
    [InlineData(0b00000001, 0b11111110)]
    [InlineData(0b00000010, 0b11111101)]
    [InlineData(0b10000000, 0b01111111)]
    [InlineData(0b11111111, 0b00000000)]
    public void SetFlag_ToggleFlagsCorrectly(ulong bitMask, ulong expectedValue)
    {
        Half bitMask_Half = Half.CreateChecked(bitMask);
        Half expectedValue_Half = Half.CreateChecked(expectedValue);
        Half allBitsSet_Half = Half.CreateChecked(0b11111111);

        // Arrange
        var group = new OutputValuesGroup<Half>(3);
        group.SetFlag(0, allBitsSet_Half, true);
        group.SetFlag(1, allBitsSet_Half, true);
        group.SetFlag(2, allBitsSet_Half, true);

        // Act
        group.SetFlag(1, bitMask_Half, false);

        // Assert
        var result = group.TryGetValues(out var values);
        result.Should().BeTrue();
        values.Length.Should().Be(3);
        values[0].Should().BeEquivalentTo(allBitsSet_Half);
        values[1].Should().BeEquivalentTo(expectedValue_Half);
        values[2].Should().BeEquivalentTo(allBitsSet_Half);
    }
}
