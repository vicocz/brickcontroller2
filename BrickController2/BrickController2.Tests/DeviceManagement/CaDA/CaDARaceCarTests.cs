using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.CaDA;
using BrickController2.PlatformServices.BluetoothLE;
using FluentAssertions;
using Moq;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.CaDA;
public class CaDARaceCarRev2Tests
{
    private readonly Mock<ICaDADeviceManager> _deviceManager = new(MockBehavior.Strict);

//TODO    [Theory]
    //[InlineData(0x76, 0x40, 0x20, 0xB9, 0x32, 0x32, 0xB2)] //AA111120B97640 323200B2A1 CCB892A0 
    public void TryGetTelegram_Connect_ReturnsProperDatagram(byte appId1, byte appId2, byte deviceId1, byte deviceId2,
        byte v1, byte v2, byte v4)
    {
        // arrange
        var device = Create<PlatformService.Default>(appId1, appId2, deviceId1: deviceId1, deviceId2: deviceId2);

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

    private CaDARaceCar Create<TPlatformService>(byte appId1, byte appId2, byte sequence = 0xA1, byte deviceId1 = 0x20, byte deviceId2 = 0x89)
        where TPlatformService : ICaDAPlatformService, new()
    {
        _deviceManager.Setup(m => m.GetAppId())
            .Returns(new byte[] { appId1, appId2 });

        return new CaDARaceCar("RC",
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
            new TPlatformService());
    }
}

