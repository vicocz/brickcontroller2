using BrickController2.DeviceManagement.CaDA;
using FluentAssertions;
using System;
using Xunit;

using static System.Half;

namespace BrickController2.Tests.DeviceManagement.CaDA;

public class RaceCarMessageEncoderRev2Tests
{
    [Theory]
    [InlineData(0xB920, 0x4076, 0x32, 0x32, 0xB2)] //AA111120B97640 323200B2A1 CCB892A0 
    public void EncodeValues_Connect_ReturnsProperPayload(ushort deviceId, ushort appId,
        byte v1, byte v2, byte v4)
    {
        // arrange
        var encoder = Create(deviceId, appId);

        // act
        var result = encoder.EncodeValues([Zero, Zero, Zero], true);

        // assert
        result.Length.Should().Be(16);
        result.ToArray().Should().Equal(
        [
            0xAA, 0x11, 0x11,
            (byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF),
            (byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF),
            v1, v2, 0x00, v4,
            0xA1, 0xCC, 0xB8, 0x92, 0xA0
        ]);
    }

    [Theory]
    [InlineData(0xB920, 0x42AD, 0x8C, 0x8C, 0x0C)] //BB111120B9AD42 8C8C000C A1CCB892B0
    [InlineData(0xB920, 0x5188, 0x76, 0x76, 0xF6)] //BB111120B98851 767600F6 A1CCB892B0 
    [InlineData(0xB920, 0x4076, 0x53, 0x53, 0xD3)] //BB111120B97640 535300D3 A1CCB892B0 
    [InlineData(0xC1C9, 0xA4B7, 0xa9, 0xa9, 0x29)] // bb 11 11 c9 c1 b7 a4  a9 a9 00 29 a1  cc b8 92 b0
    public void EncodeValues_WithZeroValues_ReturnsProperPayload(ushort deviceId, ushort appId,
        byte v1, byte v2, byte v4)
    {
        // arrange
        var encoder = Create(deviceId, appId);

        // act
        var result = encoder.EncodeValues([Zero, Zero, Zero], false);

        // assert
        result.Length.Should().Be(16);
        result.ToArray().Should().Equal(
        [
            0xBB, 0x11, 0x11,
            (byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF),
            (byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF),
            v1, v2, 0x00, v4,
            0xA1, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    [Theory]
    [InlineData(0xB920, 0x4076, 0x54, 0x54, 0xD4)] //BB111120B97640 545401D4A1 CCB892B0 
    [InlineData(0xC1C9, 0xA4B7, 0xaa, 0xaa, 0x2a)] // bb 11 11 c9 c1 b7 a4  aa aa 01 2a  a1 cc b8 92 b0 
    public void EncodeValues_WithZeroValuesAndLightOn_ReturnsProperPayload(ushort deviceId, ushort appId,
        byte v1, byte v2, byte v4)
    {
        // arrange
        var encoder = Create(deviceId, appId);

        // act
        var result = encoder.EncodeValues([Zero, Zero, One]);

        // assert
        result.Length.Should().Be(16);
        result.ToArray().Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            (byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF),
            (byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF),
            v1, v2, 0x01, v4,
            0xA1, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    [Theory]
    [InlineData(0xB920, 0x4076, -1.00f, 0x3E, 0xBE, 0xBE, 0x0B)] //BB111120B97640 3EBE01BE0B CCB892B0
    [InlineData(0xB920, 0x4076, -0.75f, 0xFE, 0x5E, 0x7E, 0xAB)] //BB111120B97640 FE5E017EAB CCB892B0 
    [InlineData(0xC1C9, 0xA4B7, -0.75f, 0x4a, 0xea, 0xca, 0xa1)] //bb 11 11 c9 c1 b7 a4  4a ea 01 ca a1  cc b8 92 b0
    [InlineData(0xC1C9, 0x2979, -0.75f, 0x9b, 0x3b, 0x1b, 0xab)] //bb 11 11 c9 c1 79 29  9b 3b 01 1b ab  cc b8 92 b0
    [InlineData(0xB920, 0x4076, 1.000f, 0xAA, 0xD5, 0x2A, 0x78)] //BB111120B97640 AAD5BE0178 CCB892B0
    [InlineData(0xC1C9, 0xA4B7, 0.746f, 0x9d, 0xc2, 0x1d, 0x35)] // bb 11 11 c9 c1 b7 a4  9d c2 01 1d 35  cc b8 92 b0
    [InlineData(0xC1C9, 0x2979, 0.746f, 0x1b, 0x44, 0x9b, 0x6c)] // bb 11 11 c9 c1 79 29  1b 44 01 9b 6c  cc b8 92 b0
    public void EncodeValues_WithPartialSteeringAndLightOn_ReturnsProperPayload(ushort deviceId, ushort appId,
        float value, byte v1, byte v2, byte v4, byte sequence)
    {
        // arrange
        var encoder = Create(deviceId, appId, sequence: (byte)(sequence - 1));

        // act
        var result = encoder.EncodeValues([Zero, (Half)value, One]);

        // assert
        result.Length.Should().Be(16);
        result.ToArray().Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            (byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF),
            (byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF),
            v1, v2, 0x01, v4,
            sequence, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    [Theory]
    [InlineData(0xC1C9, 0x2979, 1.000f, 0x4A, 0xCA, 0x4A, 0xFA)] // bb 11 11 c9 c1 79 29  4a ca 01 4a fa  cc b8 92 b0
    public void EncodeValues_WithPartialSpeedAndLightOn_ReturnsProperPayload(ushort deviceId, ushort appId,
        float value, byte v1, byte v2, byte v4, byte sequence)
    {
        // arrange
        var encoder = Create(deviceId, appId, sequence: (byte)(sequence - 1));

        // act
        var result = encoder.EncodeValues([(Half)value, Zero, One]);

        // assert
        result.Length.Should().Be(16);
        result.ToArray().Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            (byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF),
            (byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF),
            v1, v2, 0x01, v4,
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
        var result = encoder.EncodeValues([(Half)value, Zero, Zero]);

        // assert
        result.Length.Should().Be(16);
        result.ToArray().Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            (byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF),
            (byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF),
            v1, v2, 0x00, v4,
            sequence, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    private static RaceCarMessageEncoderRev2 Create(ushort deviceId, ushort appId, byte sequence = 0xA1)
        => new(new PlatformService.Default(),
            deviceId: [(byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF)],
            appId: [(byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF)],
            sequence);
}
