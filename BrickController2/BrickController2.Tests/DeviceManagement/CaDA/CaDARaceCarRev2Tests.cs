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
                0x88, 0x51,
                // some identifying bytes
                0x20, 0xB9,
                // some flag
                0x86,
                // other data
                0x00, 0x00, 0x00, 0xA1, 0xCC, 0xB8, 0x92, 0xA0
            ],
            Mock.Of<IDeviceRepository>(MockBehavior.Strict),
            Mock.Of<IBluetoothLEService>(MockBehavior.Strict),
            _cadaPlatformService.Object);
    }

    [Fact]
    public void TryGetTelegram_ZeroValuesAndIosPlatform_ReturnsProperDatagram()
    {
        // arrange
        _cadaPlatformService.TryGetRfPayload_ForIosPlatform();

        var result = _device.TryGetTelegram(false, out var telegram);

        result.Should().BeTrue();
        telegram.Should().StartWith(new byte[]
        {
            // CADA SMART CAR Rev2
            0xC0, 0x00, 0xBB, 0x11, 0x11,
            0x20, 0xB9,
            0x88, 0x51,
            //0x76, 0x76, 0x00, 0xF6, 0xA1, 0xCC, 0xB8,
            //0x92, 0xB0, 0x27, 0x0F, 0x86, 0x28, 0x17, 0xD3,
            //0xAC, 0xCB
        });
    }
}
