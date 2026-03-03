using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.CaDA;
using BrickController2.PlatformServices.BluetoothLE;
using FluentAssertions;
using Moq;
using System;
using System.Buffers.Binary;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.CaDA;

public class CaDARaceCarRev2Tests
{
    private readonly Mock<ICaDADeviceManager> _deviceManager = new(MockBehavior.Strict);

    [Theory]
    [InlineData(0x76, 0x40, 0x20, 0xB9, 0x32, 0x32, 0xB2)] //AA111120B97640 323200B2A1 CCB892A0 
    public void TryGetTelegram_Connect_ReturnsProperDatagram(byte appId1, byte appId2, byte deviceId1, byte deviceId2,
        byte v1, byte v2, byte v4)
    {
        // arrange
        var device = Create(appId1, appId2, deviceId1: deviceId1, deviceId2: deviceId2);

        // act
        var result = device.TryGetTelegram(true, out var telegram);

        // assert
        result.Should().BeTrue();
        telegram.Should().BeEquivalentTo(
        [
            0xAA, 0x11, 0x11,
            deviceId1, deviceId2,
            appId1, appId2,
            v1, v2, 0x00, v4,
            0xA1, 0xCC, 0xB8, 0x92, 0xA0
        ]);
    }

    [Theory]
    [InlineData(0xAD, 0x42, 0x20, 0xB9, 0x8C, 0x8C, 0x0C)] //BB111120B9AD42 8C8C000C A1CCB892B0
    [InlineData(0x88, 0x51, 0x20, 0xB9, 0x76, 0x76, 0xF6)] //BB111120B98851 767600F6 A1CCB892B0 
    [InlineData(0x76, 0x40, 0x20, 0xB9, 0x53, 0x53, 0xD3)] //BB111120B97640 535300D3 A1CCB892B0 
    [InlineData(0xb7, 0xa4, 0xc9, 0xc1, 0xa9, 0xa9, 0x29)] // bb 11 11 c9 c1 b7 a4  a9 a9 00 29 a1  cc b8 92 b0
    public void TryGetTelegram_WithZeroValues_ReturnsProperDatagram(byte appId1, byte appId2, byte deviceId1, byte deviceId2,
        byte v1, byte v2, byte v4)
    {
        // arrange
        var device = Create(appId1, appId2, deviceId1: deviceId1, deviceId2: deviceId2);

        // act
        var result = device.TryGetTelegram(false, out var telegram);

        // assert
        result.Should().BeTrue();
        telegram.Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            deviceId1, deviceId2,
            appId1, appId2,
            v1, v2, 0x00, v4,
            0xA1, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    [Theory]
    [InlineData(0x76, 0x40, 0x20, 0xB9, 0x54, 0x54, 0xD4)] //BB111120B97640 545401D4A1 CCB892B0 
    [InlineData(0xb7, 0xa4, 0xc9, 0xc1, 0xaa, 0xaa, 0x2a)] // bb 11 11 c9 c1 b7 a4  aa aa 01 2a  a1 cc b8 92 b0 
    public void TryGetTelegram_WithZeroValuesAndLightOn_ReturnsProperDatagram(byte appId1, byte appId2, byte deviceId1, byte deviceId2,
        byte v1, byte v2, byte v4)
    {
        // arrange
        var device = Create(appId1, appId2, deviceId1: deviceId1, deviceId2: deviceId2);
        device.SetOutput(2, 1.0f);

        // act
        var result = device.TryGetTelegram(false, out var telegram);

        // assert
        result.Should().BeTrue();
        telegram.Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            deviceId1, deviceId2,
            appId1, appId2,
            v1, v2, 0x01, v4,
            0xA1, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    [Theory]
    [InlineData(0x76, 0x40, 0x20, 0xB9, -1.00f, 0x3E, 0xBE, 0xBE, 0x0B)] //BB111120B97640 3EBE01BE0B CCB892B0
    [InlineData(0x76, 0x40, 0x20, 0xB9, -0.75f, 0xFE, 0x5E, 0x7E, 0xAB)] //BB111120B97640 FE5E017EAB CCB892B0 
    [InlineData(0xb7, 0xa4, 0xc9, 0xc1, -0.75f, 0x4a, 0xea, 0xca, 0xa1)] //bb 11 11 c9 c1 b7 a4  4a ea 01 ca a1  cc b8 92 b0
    [InlineData(0x79, 0x29, 0xc9, 0xc1, -0.75f, 0x9b, 0x3b, 0x1b, 0xab)] //bb 11 11 c9 c1 79 29  9b 3b 01 1b ab  cc b8 92 b0
    [InlineData(0x76, 0x40, 0x20, 0xB9, 1.000f, 0xAA, 0xD5, 0x2A, 0x78)] //BB111120B97640 AAD5BE0178 CCB892B0
    [InlineData(0xb7, 0xa4, 0xc9, 0xc1, 0.746f, 0x9d, 0xc2, 0x1d, 0x35)] // bb 11 11 c9 c1 b7 a4  9d c2 01 1d 35  cc b8 92 b0
    [InlineData(0x79, 0x29, 0xc9, 0xc1, 0.746f, 0x1b, 0x44, 0x9b, 0x6c)] // bb 11 11 c9 c1 79 29  1b 44 01 9b 6c  cc b8 92 b0
    public void TryGetTelegram_WithPartialSteeringAndLightOn_ReturnsProperDatagram(byte appId1, byte appId2, byte deviceId1, byte deviceId2,
        float value, byte v1, byte v2, byte v4, byte v5)
    {
        // arrange
        var device = Create(appId1, appId2, deviceId1: deviceId1, deviceId2: deviceId2, sequence:(byte)(v5 - 1));
        device.SetOutput(1, value); // #2
        device.SetOutput(2, 1.0f); // Light ON

        // act
        var result = device.TryGetTelegram(false, out var telegram);

        // assert
        result.Should().BeTrue();
        telegram.Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            deviceId1, deviceId2,
            appId1, appId2,
            v1, v2, 0x01, v4,
            v5, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    [Theory]
    [InlineData(0x79, 0x29, 0xc9, 0xc1, 1.000f, 0x4A, 0xCA, 0x4A, 0xFA)] // bb 11 11 c9 c1 79 29  4a ca 01 4a fa  cc b8 92 b0
    public void TryGetTelegram_WithPartialSpeedAndLightOn_ReturnsProperDatagram(byte appId1, byte appId2,
        byte deviceId1, byte deviceId2,
        float value, byte v1, byte v2, byte v4, byte v5)
    {
        // arrange
        var device = Create(appId1, appId2, deviceId1: deviceId1, deviceId2: deviceId2, sequence: (byte)(v5 - 1));
        device.SetOutput(0, value); // #1
        device.SetOutput(2, 1.0f); // Light ON

        // act
        var result = device.TryGetTelegram(false, out var telegram);

        // assert
        result.Should().BeTrue();
        telegram.Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            deviceId1, deviceId2,
            appId1, appId2,
            v1, v2, 0x01, v4,
            v5, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    [Theory]
    [InlineData(0xB7, 0xA4, 0xc9, 0xc1, 0.75f, 0x02, 0xa2, 0x22, 0xFA)] // bb 11 11 c9 c1 b7 a4  02 a2 00 22 fa  cc b8 92 b0
    public void TryGetTelegram_WithMiddleFirstChannel_ReturnsProperDatagram(byte appId1, byte appId2,
        byte deviceId1, byte deviceId2,
        float value, byte v1, byte v2, byte v4, byte v5)
    {
        // arrange
        var device = Create(appId1, appId2, deviceId1: deviceId1, deviceId2: deviceId2, sequence: (byte)(v5 - 1));
        device.SetOutput(0, value); // #1

        // act
        var result = device.TryGetTelegram(false, out var telegram);

        // assert
        result.Should().BeTrue();
        telegram.Should().BeEquivalentTo(
        [
            0xBB, 0x11, 0x11,
            deviceId1, deviceId2,
            appId1, appId2,
            v1, v2, 0x00, v4,
            v5, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    [Theory]
    [InlineData(0xAD, 0x42, 0x20, 0xB9, 0x8C, 0x8C, 0x0C)] //C000 BB111120B9AD42 8C8C000CA1 CCB892B0 BAFD4507415C6C37
    public void TryGetTelegram_IOS_WithZeroValues_ReturnsProperDatagram(byte appId1, byte appId2, byte deviceId1, byte deviceId2,
        byte v1, byte v2, byte v4)
    {
        // arrange
        var device = Create<PlatformService.IOS>(appId1, appId2, deviceId1: deviceId1, deviceId2: deviceId2);

        // act
        var result = device.TryGetTelegram(false, out var telegram);

        // assert
        result.Should().BeTrue();
        telegram.Should().HaveCount(26);
        telegram.Should().StartWith(
        [
            0xC0, 0x00, // manufacturerId
            0xBB, 0x11, 0x11,
            deviceId1, deviceId2,
            appId1, appId2,
            v1, v2, 0x00, v4,
            0xA1, 0xCC, 0xB8, 0x92, 0xB0
        ]);
    }

    private CaDARaceCarRev2 Create(byte appId1, byte appId2, byte sequence = 0xA1, byte deviceId1 = 0x20, byte deviceId2 = 0x89)
        => Create<PlatformService.Default>(appId1, appId2, sequence, deviceId1, deviceId2);

    private CaDARaceCarRev2 Create<TPlatformService>(byte appId1, byte appId2, byte sequence = 0xA1, byte deviceId1 = 0x20, byte deviceId2 = 0x89)
        where TPlatformService : ICaDAPlatformService, new()
    {
        _deviceManager.SetupGet(m => m.AppId)
            .Returns(BinaryPrimitives.ReadUInt16LittleEndian([appId1, appId2]));

        return new CaDARaceCarRev2("RC",
            "1-2-3",
            [
                // manufacturerId
                0xAA, 0x11,
                // CADA RaceCar?
                0x11,
                // 2 bytes AppID - zeros from the scan
                0x00, 0x00,
                // Device Id
                deviceId1, deviceId2,
                // some flag(s)
                0x86, 0x00, 0x00, 0x00, 
                // sequence,
                sequence,
                // some postfix
                0xCC, 0xB8, 0x92, 0xA0
            ],
            Mock.Of<IDeviceRepository>(MockBehavior.Strict),
            Mock.Of<IBluetoothLEService>(MockBehavior.Strict),
            _deviceManager.Object,
            new TPlatformService());
    }
}

public static class PlatformService
{
    public class Default : ICaDAPlatformService
    {
        public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload) => throw new NotImplementedException();
    }

    public class IOS : ICaDAPlatformService
    {
        public static readonly byte[] SessionId = [(byte)Random.Shared.Next(), 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, (byte)Random.Shared.Next()];
        public static readonly byte[] Prefix = [0xC0, 0x00];

        public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload) => throw new NotImplementedException();

        public bool TryGetRfPayloadRev2(ReadOnlySpan<byte> rawData, out byte[] rfPayload)
        {
            rfPayload = new byte[Prefix.Length + rawData.Length + SessionId.Length];

            // prefix
            Prefix.CopyTo(rfPayload.AsSpan());
            rawData.CopyTo(rfPayload.AsSpan(Prefix.Length));
            SessionId.CopyTo(rfPayload.AsSpan(Prefix.Length + rawData.Length));

            return rawData.Length == ICaDAPlatformService.DefaultPayloadRev2Length;
        }
    }
}
