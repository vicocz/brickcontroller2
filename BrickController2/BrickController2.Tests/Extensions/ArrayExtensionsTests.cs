using BrickController2.Extensions;
using FluentAssertions;
using Xunit;

namespace BrickController2.Tests.Extensions;

public class ArrayExtensionsTests
{
    [Fact]
    public void SequenceEqual_BothArraysNull_ReturnsTrue()
    {
        ArrayExtensions.SequenceEqual<int>(null, null).Should().BeTrue();
    }

    [Fact]
    public void SequenceEqual_OneArrayNull_ReturnsFalse()
    {
        ArrayExtensions.SequenceEqual<string>(null, []).Should().BeFalse();
        ArrayExtensions.SequenceEqual(["a"], null).Should().BeFalse();
    }

    [Fact]
    public void SequenceEqual_ArraysWithSameElements_ReturnsTrue()
    {
        // Arrange
        int[] x = [1, 2, 3];
        int[] y = [1, 2, 3];

        // Act & Assert
        x.SequenceEqual(y).Should().BeTrue();
    }

    [Fact]
    public void SequenceEqual_ArraysWithDifferentElements_ReturnsFalse()
    {
        // Arrange
        int[] x = [1, 2, 3];
        int[] y = [4, 5, 6];

        // Act & Assert
        x.SequenceEqual(y).Should().BeFalse();
    }

    [Fact]
    public void SequenceEqual_ArraysWithDifferentLengths_ReturnsFalse()
    {
        // Arrange
        short[] x = [1, 2, 3];
        short[] y = { 1, 2 };

        // Act & Assert
        x.SequenceEqual(y).Should().BeFalse();
    }
}
