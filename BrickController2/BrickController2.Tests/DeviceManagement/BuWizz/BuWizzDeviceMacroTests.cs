using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.BuWizz;
using BrickController2.DeviceManagement.Macros;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Settings;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.BuWizz;

public class BuWizzDeviceMacroTests
{
    [Fact]
    public void BuWizzDevice_AvailableMacros_ReturnsSetOutputLevelWithThreeChoices()
    {
        var device = new TestBuWizzDevice();

        device.SupportsMacros.Should().BeTrue();
        device.AvailableMacros.Should().ContainSingle();

        var macro = device.AvailableMacros.Single();
        macro.Id.Should().Be("SetOutputLevel");
        macro.Scope.Should().Be(MacroScope.Device);
        macro.Kind.Should().Be(MacroKind.OneShot);
        macro.Choices.Select(c => c.Value).Should().Equal(
            (int)BuWizzOutputLevels.Low,
            (int)BuWizzOutputLevels.Normal,
            (int)BuWizzOutputLevels.High);
    }

    [Fact]
    public async Task BuWizzDevice_ExecuteMacroAsync_SetOutputLevelMacro_UsesSelectedChoiceValue()
    {
        var device = new TestBuWizzDevice();

        await device.ExecuteMacroAsync(new MacroInvocation("SetOutputLevel", (int)BuWizzOutputLevels.High, null), CancellationToken.None);

        device.LastSetOutputLevel.Should().Be((int)BuWizzOutputLevels.High);
    }

    [Fact]
    public void BuWizz2Device_AvailableMacros_ReturnsSetOutputLevelWithFourChoices()
    {
        var device = new TestBuWizz2Device();

        device.SupportsMacros.Should().BeTrue();
        device.AvailableMacros.Should().ContainSingle();

        var macro = device.AvailableMacros.Single();
        macro.Id.Should().Be("SetOutputLevel");
        macro.Scope.Should().Be(MacroScope.Device);
        macro.Kind.Should().Be(MacroKind.OneShot);
        macro.Choices.Select(c => c.Value).Should().Equal(
            (int)BuWizz2OutputLevels.Low,
            (int)BuWizz2OutputLevels.Normal,
            (int)BuWizz2OutputLevels.High,
            (int)BuWizz2OutputLevels.Ludicrous);
    }

    [Fact]
    public async Task BuWizz2Device_ExecuteMacroAsync_SetOutputLevelMacro_UsesSelectedChoiceValue()
    {
        var device = new TestBuWizz2Device();

        await device.ExecuteMacroAsync(new MacroInvocation("SetOutputLevel", (int)BuWizz2OutputLevels.Ludicrous, null), CancellationToken.None);

        device.LastSetOutputLevel.Should().Be((int)BuWizz2OutputLevels.Ludicrous);
    }

    private sealed class TestBuWizzDevice : BuWizzDevice
    {
        public TestBuWizzDevice()
            : base("test", "addr", new List<NamedSetting>(),
                  new Mock<IDeviceRepository>().Object,
                  new Mock<IBluetoothLEService>().Object)
        {
        }

        public int? LastSetOutputLevel { get; private set; }

        public override void SetOutputLevel(int value)
        {
            LastSetOutputLevel = value;
            base.SetOutputLevel(value);
        }
    }

    private sealed class TestBuWizz2Device : BuWizz2Device
    {
        public TestBuWizz2Device()
            : base("test", "addr", [0x4e, 0x05, 0x42, 0x57, 0x00, 0x1b], new List<NamedSetting>(),
                  new Mock<IDeviceRepository>().Object,
                  new Mock<IBluetoothLEService>().Object)
        {
        }

        public int? LastSetOutputLevel { get; private set; }

        public override void SetOutputLevel(int value)
        {
            LastSetOutputLevel = value;
            base.SetOutputLevel(value);
        }
    }
}
