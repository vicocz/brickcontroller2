using BrickController2.DeviceManagement.JieStar;
using BrickController2.UI.Services.AppIdentifier;
using FluentAssertions;
using Moq;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.JieStar;

public class JieStarDeviceManagerTests
{
    private const byte AppIdentifier1 = 0x61; // This is the first byte of an randomly chosen AppIdentifier for UnitTesting
    private const byte AppIdentifier2 = 0x62; // This is the second byte of an randomly chosen AppIdentifier for UnitTesting

    private readonly IJieStarDeviceManager _manager;
    private readonly Mock<IAppIdentifierService> _appIdentifierService = new(MockBehavior.Strict);

    public JieStarDeviceManagerTests()
    {
        _appIdentifierService.Setup(x => x.GetAppId(2)).Returns(new byte[] { AppIdentifier1, AppIdentifier2 });

        _manager = new JieStarDeviceManager(_appIdentifierService.Object);
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
