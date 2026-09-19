using BrickController2.DeviceManagement.PowerBox;
using BrickController2.UI.Services.AppIdentifier;
using BrickController2.UI.Services.Preferences;
using FluentAssertions;
using Moq;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.PowerBox;

public class PowerBoxDeviceManagerTests
{
    private const byte AppIdentifier1 = 0x61; // 'a' = 0x61
    private const byte AppIdentifier2 = 0x62; // 'b' = 0x62

    private readonly IPowerBoxDeviceManager _manager;
    private readonly Mock<IAppIdentifierService> _appIdentifierService = new(MockBehavior.Strict);

    public PowerBoxDeviceManagerTests()
    {
        _appIdentifierService.Setup(x => x.GetAppId(2)).Returns(new byte[] { AppIdentifier1, AppIdentifier2 });
        _manager = new PowerBoxDeviceManager(_appIdentifierService.Object);
    }

    [Fact]
    public void AppId_TwoBytesInPreferences_AllBytes()
    {
        var appId = _manager.GetAppId();
        appId.Length.Should().Be(2);
        appId.Span[0].Should().Be(AppIdentifier1);
        appId.Span[1].Should().Be(AppIdentifier2);
    }
}
