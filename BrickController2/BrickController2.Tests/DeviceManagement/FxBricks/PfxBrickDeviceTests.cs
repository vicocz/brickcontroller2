using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.FxBricks;
using BrickController2.DeviceManagement.Macros;
using BrickController2.PlatformServices.BluetoothLE;
using FluentAssertions;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.FxBricks;

public class PfxBrickDeviceTests
{
    private static PfxBrickDevice CreateDevice()
        => new("PFx", "00:11:22:33:44:55", [], Mock.Of<IDeviceRepository>(), Mock.Of<IBluetoothLEService>());

    [Fact]
    public void AvailableMacros_LightEffects_AreRepeatableChannelMacros()
    {
        // Arrange
        var device = CreateDevice();

        // Act
        var lightMacros = device.AvailableMacros.Where(m => m.Id.StartsWith("Light")).ToArray();

        // Assert
        lightMacros.Should().HaveCount(14);
        lightMacros.Should().OnlyContain(m => m.Scope == MacroScope.Channel && m.Kind == MacroKind.Repeatable && m.Choices.Count == 0);
        lightMacros.Select(m => m.Id).Should().Contain(["LightFlash50Pos", "LightLighthouse", "LightSoundModulated"]);
        lightMacros.Select(m => m.Id).Should().NotContain("LightMotorModulated");
    }

    [Theory]
    [InlineData("IncreaseBrightness")]
    [InlineData("DecreaseBrightness")]
    public void AvailableMacros_BrightnessMacros_AreOneShotChannelMacros(string macroId)
    {
        // Arrange
        var device = CreateDevice();

        // Act
        var macro = device.AvailableMacros.SingleOrDefault(m => m.Id == macroId);

        // Assert
        macro.Should().NotBeNull();
        macro!.Scope.Should().Be(MacroScope.Channel);
        macro.Kind.Should().Be(MacroKind.OneShot);
    }

    [Fact]
    public async Task ExecuteMacroAsync_LightEffectWhenDisconnected_ReturnsFalse()
    {
        // Arrange
        var device = CreateDevice();

        // Act
        var result = await device.ExecuteMacroAsync(new MacroInvocation("LightFlicker", default, 2), TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeFalse();
    }
}
