using System.Collections.ObjectModel;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement.Macros;
using FluentAssertions;
using Xunit;

namespace BrickController2.Tests.CreationManagement;

public class CreationMacroReferencesTests
{
    [Fact]
    public void GetMacroReferences_ReturnsEmpty_WhenNoMacroActions()
    {
        var creation = BuildCreation(new ControllerAction
        {
            DeviceId = "dev1",
            ButtonType = ControllerButtonType.Sequence,
            SequenceName = "seq"
        });

        creation.GetMacroReferences().Should().BeEmpty();
    }

    [Fact]
    public void GetMacroReferences_ReturnsChannelScope_ForMacroActions()
    {
        var creation = BuildCreation(new ControllerAction
        {
            DeviceId = "dev1",
            ButtonType = ControllerButtonType.Macro,
            MacroId = "SetOutputLevel"
        });

        creation.GetMacroReferences().Should().ContainSingle()
            .Which.Should().Be(("dev1", "SetOutputLevel", MacroScope.Channel));
    }

    [Fact]
    public void GetMacroReferences_ReturnsDeviceScope_ForDeviceMacroActions()
    {
        var creation = BuildCreation(new ControllerAction
        {
            DeviceId = "dev1",
            ButtonType = ControllerButtonType.DeviceMacro,
            MacroId = "Reset"
        });

        creation.GetMacroReferences().Should().ContainSingle()
            .Which.Should().Be(("dev1", "Reset", MacroScope.Device));
    }

    [Fact]
    public void GetMacroReferences_DeduplicatesByDeviceMacroScope()
    {
        var creation = BuildCreation(
            new ControllerAction { DeviceId = "dev1", ButtonType = ControllerButtonType.Macro, MacroId = "m" },
            new ControllerAction { DeviceId = "dev1", ButtonType = ControllerButtonType.Macro, MacroId = "m" },
            new ControllerAction { DeviceId = "dev1", ButtonType = ControllerButtonType.DeviceMacro, MacroId = "m" });

        var references = creation.GetMacroReferences();

        references.Should().HaveCount(2);
        references.Should().Contain(("dev1", "m", MacroScope.Channel));
        references.Should().Contain(("dev1", "m", MacroScope.Device));
    }

    [Fact]
    public void GetMacroReferences_IgnoresMacroActions_WithEmptyMacroId()
    {
        var creation = BuildCreation(new ControllerAction
        {
            DeviceId = "dev1",
            ButtonType = ControllerButtonType.Macro,
            MacroId = string.Empty
        });

        creation.GetMacroReferences().Should().BeEmpty();
    }

    private static Creation BuildCreation(params ControllerAction[] actions)
    {
        var controllerEvent = new ControllerEvent
        {
            ControllerActions = new ObservableCollection<ControllerAction>(actions)
        };
        var profile = new ControllerProfile
        {
            ControllerEvents = new ObservableCollection<ControllerEvent> { controllerEvent }
        };
        return new Creation
        {
            ControllerProfiles = new ObservableCollection<ControllerProfile> { profile }
        };
    }
}
