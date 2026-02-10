using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.CaDA;
using BrickController2.PlatformServices.BluetoothLE;
using FluentAssertions;
using Moq;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.CaDA;

public class CaDARaceCarRev2Tests
{
    private readonly CaDARaceCarRev2 _device;
    private readonly Mock<ICaDAPlatformService> _cadaPlatformService = new(MockBehavior.Strict);

    public CaDARaceCarRev2Tests()
    {
        _device = new CaDARaceCarRev2("RC",
            "1-2-3",
            [
                // manufacturerId
                0xAA, 0x11,
                // CADA RaceCar?
                0x11,
                // 2 bytes AppID
                0x00, 0x00,
                // Device Seed
                0x20, 0xB9,
                // some flag(s)
                0x86, 0x00, 0x00, 0x00, 
                // hardware id
                0xA1, 0xCC, 0xB8, 0x92, 0xA0
            ],
            Mock.Of<IDeviceRepository>(MockBehavior.Strict),
            Mock.Of<IBluetoothLEService>(MockBehavior.Strict),
            _cadaPlatformService.Object);
    }

    [Fact]
    public void TryGetTelegram_ConnectTelegram_ReturnsProperDatagram()
    {
        // arrange
        _cadaPlatformService.TryGetRfPayload_ForIosPlatform();

        var result = _device.TryGetTelegram(true, out var telegram);

        result.Should().BeTrue();
        telegram.Should().StartWith(new byte[]
        {
            0xC0, 0x00, 0xAA, 0x11, 0x11,
            0x20, 0xB9,
            0xAD, 0x42,
            0x6B, 0x6B, 0x00, /*0xEB,*/ 0xD6, //TODO checksum
            0xA1, 0xCC, 0xB8, 0x92, 0xA0,
            0xEF, 0xF2, 0xC5, 0x67, 0x8F, 0x9F, 0xF1, 0xF8
        });
    }

    [Fact]
    public void TryGetTelegram_WithZeroValuesTelegram_ReturnsProperDatagram()
    {
        // arrange
        _cadaPlatformService.TryGetRfPayload_ForIosPlatform();

        var result = _device.TryGetTelegram(false, out var telegram);

        result.Should().BeTrue();
        telegram.Should().StartWith(new byte[]
        {
            0xC0, 0x00, 0xBB, 0x11, 0x11,
            0x20, 0xB9,
            0xAD, 0x42,
            0x80, 0x80, 0x80, 0x80, //TODO checksum
            0xA1, 0xCC, 0xB8, 0x92, 0xB0,
            0xEF, 0xF2, 0xC5, 0x67, 0x8F, 0x9F, 0xF1, 0xF8
        });
    }
}
