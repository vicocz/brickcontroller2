using BrickController2.DeviceManagement.CaDA;
using FluentAssertions;
using System;
using Xunit;

using static System.Half;

namespace BrickController2.Tests.DeviceManagement.CaDA;

public class RaceCarMessageEncoderRev2Tests
{
    [Theory]
    [InlineData(0xb920, 0x4076, 0x32, 0x32, 0xb2)] // aa 11 11 20 b9 76 40 32 32 00 b2  a1 cc b8 92 a0 
    public void EncodeValues_Connect_ReturnsProperPayload(ushort deviceId, ushort appId,
        byte v1, byte v2, byte v4)
    {
        // arrange
        var encoder = Create(deviceId, appId);

        // act
        var result = encoder.EncodeValues([Zero, Zero, Zero, Zero], true);

        // assert
        result.Length.Should().Be(16);
        result.ToArray().Should().Equal(
        [
            0xAA, 0x11, 0x11,
            (byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF),
            (byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF),
            v1, v2, 0b00, v4,
            0xA1, 0xCC, 0xB8, 0x92, 0xA0
        ]);
    }

    [Theory]
    [InlineData(0xb920, 0x42ad, 0x8c, 0x8c, 0x0c)] // bb 11 11 20 b9 ad 42  8c 8c 00 0c  a1 cc b8 92 b0
    [InlineData(0xb920, 0x5188, 0x76, 0x76, 0xf6)] // bb 11 11 20 b9 88 51  76 76 00 f6  a1 cc b8 92 b0 
    [InlineData(0xb920, 0x4076, 0x53, 0x53, 0xd3)] // bb 11 11 20 b9 76 40  53 53 00 d3  a1 cc b8 92 b0 
    [InlineData(0xc1c9, 0xa4b7, 0xa9, 0xa9, 0x29)] // bb 11 11 c9 c1 b7 a4  a9 a9 00 29  a1 cc b8 92 b0
    public void EncodeValues_WithZeroValues_ReturnsProperPayload(ushort deviceId, ushort appId,
        byte v1, byte v2, byte v4)
    {
        // arrange
        var encoder = Create(deviceId, appId);

        // act
        var result = encoder.EncodeValues([Zero, Zero, Zero, Zero], false);

        // assert
        result.Length.Should().Be(16);
        result.ToArray().Should().Equal(
        [
            0xBB, 0x11, 0x11,
            (byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF),
            (byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF),
            v1, v2, 0b00, v4,
            0xA1, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    [Theory]
    [InlineData(0xb920, 0x4076, 0x56, 0x56, 0xd6)] // bb 11 11 20 b9 76 40  56 56 03 d6  a1 cc b8 92 b0 
    [InlineData(0xc1c9, 0xa4b7, 0xac, 0xac, 0x2c)] // bb 11 11 c9 c1 b7 a4  ac ac 03 2c  a1 cc b8 92 b0 
    public void EncodeValues_WithZeroValuesAndFrontLightOnAndRearLightOn_ReturnsProperPayload(ushort deviceId, ushort appId,
        byte v1, byte v2, byte v4)
    {
        // arrange
        var encoder = Create(deviceId, appId);

        // act
        var result = encoder.EncodeValues([Zero, Zero, One, One]);

        // assert
        result.Length.Should().Be(16);
        result.ToArray().Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            (byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF),
            (byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF),
            v1, v2, 0b11, v4,
            0xA1, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    [Theory]
    [InlineData(0xb920, 0x4076, -1.00f, 0x40, 0xc0, 0xc0, 0x0b)] // bb 11 11 20 b9 76 40  40 c0 03 c0 0b  cc b8 92 b0
    [InlineData(0xb920, 0x4076, -0.75f, 0x00, 0xa0, 0x80, 0xab)] // bb 11 11 20 b9 76 40  00 a0 03 80 ab  cc b8 92 b0 
    [InlineData(0xc1c9, 0xa4b7, -0.75f, 0x4c, 0xec, 0xcc, 0xa1)] // bb 11 11 c9 c1 b7 a4  4c ec 03 cc a1  cc b8 92 b0
    [InlineData(0xc1c9, 0x2979, -0.75f, 0x9d, 0x3d, 0x1d, 0xab)] // bb 11 11 c9 c1 79 29  9d 3d 03 1d ab  cc b8 92 b0
    [InlineData(0xb920, 0x4076, 1.000f, 0xac, 0xd3, 0x2c, 0x78)] // bb 11 11 20 b9 76 40  aa ac d3 03 2c  cc b8 92 b0
    [InlineData(0xc1c9, 0xa4b7, 0.746f, 0x9f, 0xc0, 0x1f, 0x35)] // bb 11 11 c9 c1 b7 a4  9f c0 03 1f 35  cc b8 92 b0
    [InlineData(0xc1c9, 0x2979, 0.746f, 0x1d, 0x42, 0x9d, 0x6c)] // bb 11 11 c9 c1 79 29  1d 42 03 9d 6c  cc b8 92 b0
    public void EncodeValues_WithPartialSteeringAndFrontLightOnAndRearLightOn_ReturnsProperPayload(ushort deviceId, ushort appId,
        float value, byte v1, byte v2, byte v4, byte sequence)
    {
        // arrange
        var encoder = Create(deviceId, appId, sequence: (byte)(sequence - 1));

        // act
        var result = encoder.EncodeValues([Zero, (Half)value, One, One]);

        // assert
        result.Length.Should().Be(16);
        result.ToArray().Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            (byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF),
            (byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF),
            v1, v2, 0b11, v4,
            sequence, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    [Theory]
    [InlineData(0xC1C9, 0x2979, 1.000f, 0x4C, 0xCC, 0x4C, 0xFA)] // bb 11 11 c9 c1 79 29  4a ca 01 4a fa  cc b8 92 b0
    public void EncodeValues_WithPartialSpeedAndFrontLightOnAndRearLightOn_ReturnsProperPayload(ushort deviceId, ushort appId,
        float value, byte v1, byte v2, byte v4, byte sequence)
    {
        // arrange
        var encoder = Create(deviceId, appId, sequence: (byte)(sequence - 1));

        // act
        var result = encoder.EncodeValues([(Half)value, Zero, One, One]);

        // assert
        result.Length.Should().Be(16);
        result.ToArray().Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            (byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF),
            (byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF),
            v1, v2, 0b11, v4,
            sequence, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    [Theory]
    [InlineData(0xC1C9, 0xA4B7, 0.75f, 0x02, 0xa2, 0x22, 0xFA)] // bb 11 11 c9 c1 b7 a4  02 a2 00 22 fa  cc b8 92 b0
    public void EncodeValues_WithMiddleFirstChannel_ReturnsProperPayload(ushort deviceId, ushort appId,
        float value, byte v1, byte v2, byte v4, byte sequence)
    {
        // arrange
        var encoder = Create(deviceId, appId, sequence: (byte)(sequence - 1));

        // act
        var result = encoder.EncodeValues([(Half)value, Zero, Zero, Zero]);

        // assert
        result.Length.Should().Be(16);
        result.ToArray().Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            (byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF),
            (byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF),
            v1, v2, 0b00, v4,
            sequence, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    [Theory]
    [InlineData(0xC1C9, 0x2979, 0x4A, 0x4A, 0xCA, 0xFA)] // bb 11 11 c9 c1 79 29  4a 4a 01 ca fa  cc b8 92 b0
    public void EncodeValues_WithZeroValuesAndFrontLightOnAndRearLightOff_ReturnsProperPayload(ushort deviceId, ushort appId,
        byte v1, byte v2, byte v4, byte sequence)
    {
        // arrange
        var encoder = Create(deviceId, appId, sequence: sequence); // speed and steering are zero, so the sequence is not incremented

        // act
        var result = encoder.EncodeValues([Zero, Zero, One, Zero]);

        // assert
        result.Length.Should().Be(16);
        result.ToArray().Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            (byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF),
            (byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF),
            v1, v2, 0b01, v4,
            sequence, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    [Theory]
    [InlineData(0xC1C9, 0x2979, 0x4B, 0x4B, 0xCB, 0xFA)] // bb 11 11 c9 c1 79 29  4b 4b 03 cb fa  cc b8 92 b0
    public void EncodeValues_WithZeroValuesAndFrontLightOffAndRearLightOn_ReturnsProperPayload(ushort deviceId, ushort appId,
        byte v1, byte v2, byte v4, byte sequence)
    {
        // arrange
        var encoder = Create(deviceId, appId, sequence: sequence); // speed and steering are zero, so the sequence is not incremented

        // act
        var result = encoder.EncodeValues([Zero, Zero, Zero, One]);

        // assert
        result.Length.Should().Be(16);
        result.ToArray().Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            (byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF),
            (byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF),
            v1, v2, 0b10, v4,
            sequence, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    private static RaceCarMessageEncoderRev2 Create(ushort deviceId, ushort appId, byte sequence = 0xA1)
        => new(new PlatformService.Default(),
            deviceId: [(byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF)],
            appId: [(byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF)],
            sequence);
}
