using BrickController2.CreationManagement;
using BrickController2.Extensions;
using FluentAssertions;
using Xunit;

namespace BrickController2.Tests.Extensions;

public class ChannelOutputTypeExtensionsTests
{
    [Theory]
    [InlineData(ChannelOutputType.ServoMotor, true)]
    [InlineData(ChannelOutputType.StepperMotor, true)]
    [InlineData(ChannelOutputType.NormalMotor, false)]
    [InlineData((ChannelOutputType)999, false)]
    public void IsChannelSetupSupported_ShouldReturnExpectedResult(ChannelOutputType outputType, bool expected)
    {
        // Act
        var result = outputType.IsChannelSetupSupported();

        // Assert
        result.Should().Be(expected);
    }
}
