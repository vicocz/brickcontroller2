using BrickController2.DeviceManagement.CaDA;
using FluentAssertions;
using Moq;
using System;
using Xunit;

using static System.Half;

namespace BrickController2.Tests.DeviceManagement.CaDA;

public class RaceCarMessageEncoderTests
{
    private readonly Mock<Random> _random = new(MockBehavior.Strict);

    [Fact]
    public void Encode_WithValidInput_ReturnsCorrectLengthAndStaticStructure()
    {
        // Arrange
        var encoder = Create(0x4032, [0x01, 0x23, 0x40], [0x87, 0x65, 0x43]);

        // Act
        var result = encoder.Encode([Zero, Zero, Zero]);

        // Assert
        result.Length.Should().Be(16);
        result.Should().StartWith(
        [
            0x75, // first byte is constant 0x75
            0x13, // second byte is constant 0x13 (STATUS_CONTROL)
            0x01, // device address byte 1
            0x23, // device address byte 2
            0x40, // device address byte 3
            0x87, // app ID byte 1
            0x65, // app ID byte 2
            0x43  // app ID byte 3
        ]);
    }

    [Fact]
    public void Encode_WithFullLightValue_EncodesAndEncryptsValues()
    {
        // Arrange
        var encoder = Create(0x4032, [0x01, 0x02, 0x03], [0x04, 0x05, 0x06]);

        // Act
        var result = encoder.Encode([Zero, Zero, One]);

        // Assert
        result.Length.Should().Be(16);
        result.Should().EndWith([0xD6, 0xA4, 0x25, 0x89, 0x4E, 0x6D, 0x25, 0x25]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(10)]
    public void Encode_WithInvalidValueCount_ThrowsArgumentException(int count)
    {
        // Arrange
        var encoder = Create(0x4032, [0x01, 0x02, 0x03], [0x04, 0x05, 0x06]);

        // Act
        Action act = () => encoder.Encode(new Half[count]);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid input data.*")
            .And.ParamName.Should().Be("values");
    }

    [Fact]
    public void EncodeValues_WithZeros_ShouldEncodeCorrectly()
    {
        // Arrange
        var encoder = Create(0xABCD, [0x01, 0x02, 0x03], [0x04, 0x05, 0x06]);
        // Act
        var result = encoder.EncodeValues([Zero, Zero, Zero]);
        // Assert
        result.Length.Should().Be(8);
        result.ToArray().Should().Equal(
        [
            0xCD, //  [8] ChannelData random
            0xAB, //  [9] ChannelData random
            0x80, // [10] ChannelData verticalValue (min= 0x80 (128))
            0x80, // [11] ChannelData horizontalValue (min= 0x80 (128))
            0x80, // [12] ChannelData lightValue
            0x00, // [13] ChannelData 
            0x00, // [14] ChannelData 
            0x00, // [15] ChannelData 
        ]);
    }

    [Theory]
    [InlineData(-2.0f, 1.0f, 1.0f)]  // Out of range speed
    [InlineData(-1.0f, 2.0f, 1.0f)]  // Out of range steering
    [InlineData(-1.0f, 1.0f, 2.0f)]  // Out of range light
    public void Encode_WithOutOfRangeValues_ClampsTo0xFF(float speed, float steering, float light)
    {
        // Arrange
        var encoder = Create();
        // Act
        var result = encoder.EncodeValues([(Half)speed, (Half)steering, (Half)light]);
        // Assert
        result.ToArray().Should().EndWith(
        [
            0xFF, // [10] ChannelData verticalValue (min= 0x80 (128))
            0xFF, // [11] ChannelData horizontalValue (min= 0x80 (128))
            0xFF, // [12] ChannelData lightValue
            0x00, // [13] ChannelData 
            0x00, // [14] ChannelData 
            0x00, // [15] ChannelData 
        ]);
    }

    [Theory]
    [InlineData(2.0f, -1.0f, -1.0f)]  // Out of range speed
    [InlineData(1.0f, -2.0f, -1.0f)]  // Out of range steering
    [InlineData(1.0f, -1.0f, -2.0f)]  // Out of range light
    public void Encode_WithOutOfRangeValues_ClampsTo0x00(float speed, float steering, float light)
    {
        // Arrange
        var encoder = Create();
        // Act
        var result = encoder.EncodeValues([(Half)speed, (Half)steering, (Half)light]);
        // Assert
        result.ToArray().Should().EndWith(
        [
            0x00, // [10] ChannelData verticalValue (min= 0x80 (128))
            0x00, // [11] ChannelData horizontalValue (min= 0x80 (128))
            0x00, // [12] ChannelData lightValue
            0x00, // [13] ChannelData 
            0x00, // [14] ChannelData 
            0x00, // [15] ChannelData 
        ]);
    }

    private RaceCarMessageEncoder Create() => Create(0x4032, [0x01, 0x02, 0x03], [0x04, 0x05, 0x06]);

    private RaceCarMessageEncoder Create(ushort random, ReadOnlySpan<byte> deviceAddress, ReadOnlySpan<byte> appId)
    {
        _random.Setup(r => r.Next(ushort.MinValue, ushort.MaxValue)).Returns(random);
        return new(new PlatformService.Default(), _random.Object, deviceAddress, appId);
    }
}