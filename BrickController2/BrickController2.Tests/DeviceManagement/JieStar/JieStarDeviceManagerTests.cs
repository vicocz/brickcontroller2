using BrickController2.DeviceManagement.JieStar;
using BrickController2.UI.Services.AppIdentifier;
using BrickController2.UI.Services.Preferences;
using FluentAssertions;
using Moq;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.JieStar;

public class JieStarDeviceManagerTests
{
    private readonly JieStarDeviceManager _manager;
    private readonly Mock<IPreferencesService> _preferencesService = new(MockBehavior.Strict);

    public JieStarDeviceManagerTests()
    {
        _preferencesService.Setup(x => x.ContainsKey("Identifier", "App")).Returns(true);
        _preferencesService.Setup(x => x.Get("Identifier", "", "App")).Returns("YWJj");

        IAppIdentifierService appIdentifierService = new AppIdentifierService(_preferencesService.Object);
        _manager = new JieStarDeviceManager(appIdentifierService);
    }

    [Fact]
    public void AppId_TwoBytesInPreferences_AllBytes()
    {
        var appId = _manager.GetAppId();
        appId.Length.Should().Be(2);
        appId.Span[0].Should().Be(0x61); // 'a' = 0x61
        appId.Span[1].Should().Be(0x62); // 'b' = 0x62
    }
}
