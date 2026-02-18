using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.CaDA;
using BrickController2.PlatformServices.BluetoothLE;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.CaDA;

public class CaDARaceCarRev2Tests
{
    private static CaDARaceCarRev2 Create(byte sequence = 0xA1)
    {
        return new CaDARaceCarRev2("RC",
            "1-2-3",
            [
                // manufacturerId
                0xAA, 0x11,
                // CADA RaceCar?
                0x11,
                // 2 bytes AppID - zeros from the scan
                0x00, 0x00,
                // Device Seed
                0x20, 0xB9,
                // some flag(s)
                0x86, 0x00, 0x00, 0x00, 
                // sequence,
                sequence,
                // some postfix
                0xCC, 0xB8, 0x92, 0xA0
            ],
            Mock.Of<IDeviceRepository>(MockBehavior.Strict),
            Mock.Of<IBluetoothLEService>(MockBehavior.Strict),
            new CaDATestPlatformService());
    }

    [Theory]
    [InlineData(0x76, 0x40, 0x32, 0x32, 0x00, 0xB2)] //AA111120B97640323200B2A1CCB892A0 
    public void TryGetTelegram_ConnectTelegram_ReturnsProperDatagram(byte appId1, byte appId2, byte v1, byte v2, byte v3, byte v4)
    {
        // arrange
        var device = Create();
        device.SetAppId(appId1, appId2);

        // act
        var result = device.TryGetTelegram(true, out var telegram);

        // assert
        result.Should().BeTrue();
        telegram.Should().BeEquivalentTo(
        [
            0xAA, 0x11, 0x11,
            0x20, 0xB9,
            appId1, appId2,
            v1, v2, v3, v4,
            0xA1, 0xCC, 0xB8, 0x92, 0xA0
        ]);
    }

    [Theory]
    [InlineData(0xAD, 0x42, 0x8C, 0x8C, 0x00, 0x0C)] //BB111120B9AD428C8C000CA1CCB892B0
    [InlineData(0x88, 0x51, 0x76, 0x76, 0x00, 0xF6)] //BB111120B98851767600F6A1CCB892B0 
    [InlineData(0x76, 0x40, 0x53, 0x53, 0x00, 0xD3)] //BB111120B97640535300D3A1CCB892B0 
    public void TryGetTelegram_WithZeroValuesTelegram_ReturnsProperDatagram(byte appId1, byte appId2, byte v1, byte v2, byte v3, byte v4)
    {
        // arrange
        var device = Create();
        device.SetAppId(appId1, appId2);

        // act
        var result = device.TryGetTelegram(false, out var telegram);

        // assert
        result.Should().BeTrue();
        telegram.Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            0x20, 0xB9,
            appId1, appId2,
            v1, v2, v3, v4,
            0xA1, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    [Theory]
    [InlineData(0x76, 0x40, 0x54, 0x54, 0x01, 0xD4)] //BB111120B97640545401D4A1CCB892B0 
    public void TryGetTelegram_WithZeroValuesAndLightOnTelegram_ReturnsProperDatagram(byte appId1, byte appId2, byte v1, byte v2, byte v3, byte v4)
    {
        // arrange
        var device = Create();
        device.SetAppId(appId1, appId2);
        device.SetOutput(2, 1.0f);

        // act
        var result = device.TryGetTelegram(false, out var telegram);

        // assert
        result.Should().BeTrue();
        telegram.Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            0x20, 0xB9,
            appId1, appId2,
            v1, v2, v3, v4,
            0xA1, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }


    [Theory]
    [InlineData(0x76, 0x40, 0xAA, 0xFE, 0x5E, 0x01, 0x7E, 0xAB)] //BB111120B97640FE5E017EABCCB892B0 
    [InlineData(0x76, 0x40, 0xA3, 0xF7, 0x57, 0x01, 0x77, 0xA4)] //BB111120B97640F7570177A4CCB892B0 
    [InlineData(0x76, 0x40, 0x0A, 0x3E, 0x3E, 0x01, 0xBE, 0x0B)] //BB111120B976403EBE01BE0BCCB892B0 
    public void TryGetTelegram_WithFullSecondChannelAndLightOnTelegram_ReturnsProperDatagram(byte appId1, byte appId2, byte sequence, byte v1, byte v2, byte v3, byte v4, byte v5)
    {
        // arrange
        var device = Create(sequence);
        device.SetAppId(appId1, appId2);
        device.SetOutput(1, 1.0f); // #2
        device.SetOutput(2, 1.0f); // Light ON

        // act
        var result = device.TryGetTelegram(false, out var telegram);

        // assert
        result.Should().BeTrue();
        telegram.Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            0x20, 0xB9,
            appId1, appId2,
            v1, v2, v3, v4,
            v5, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }
}


public class CaDATestPlatformService : ICaDAPlatformService
{
    private const int PayloadRev2Length = 16;

    public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload)
    {
        throw new NotImplementedException();
    }

    public bool TryGetRfPayload(ushort manufacturerId, ReadOnlySpan<byte> rawData, out byte[] rfPayload)
    {
        // copy past data
        rfPayload = rawData.ToArray();
        return rawData.Length == PayloadRev2Length;
    }
}
