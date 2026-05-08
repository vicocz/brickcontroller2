using BrickController2.Protocols;
using FluentAssertions;
using Xunit;

namespace BrickController2.Tests.Protocols;

public class LegoWirelessProtocolTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(179)]
    [InlineData(-179)]
    [InlineData(-180)]
    [InlineData(90)]
    [InlineData(-90)]
    public void NormalizeAngle_WithinRange_ShouldReturnSameValue(int value)
    {
        var result = LegoWirelessProtocol.NormalizeAngle(value);
        result.Should().Be(value);
    }

    [Theory]
    [InlineData(180, -180)]
    [InlineData(360, 0)]
    [InlineData(540, -180)]
    public void NormalizeAngle_OverPlus180_ShouldReturnNormalizedValue(int input, int expected)
    {
        var result = LegoWirelessProtocol.NormalizeAngle(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(-541, 179)]
    [InlineData(-540, -180)]
    [InlineData(-539, -179)]
    [InlineData(-360, 0)]
    [InlineData(-181, 179)]
    public void NormalizeAngle_BelowMinus180_ShouldReturnNormalizedValue(int input, int expected)
    {
        var result = LegoWirelessProtocol.NormalizeAngle(input);
        result.Should().Be(expected);
    }
}