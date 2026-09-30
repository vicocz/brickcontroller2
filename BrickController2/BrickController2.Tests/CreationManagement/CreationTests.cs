using BrickController2.CreationManagement;
using BrickController2.DeviceManagement.Macros;
using Xunit;

namespace BrickController2.Tests.CreationManagement;

public class CreationTests
{
    [Fact]
    public void GetMacroReferences_ReturnsEmpty_WhenCreationHasNoControllerActions()
    {
        var creation = CreateCreation();

        var result = creation.GetMacroReferences();

        Assert.Empty(result);
    }

    [Fact]
    public void GetMacroReferences_ReturnsEmpty_WhenNoActionIsMacroType()
    {
        var creation = CreateCreation(
            CreateControllerAction(buttonType: ControllerButtonType.Normal),
            CreateControllerAction(buttonType: ControllerButtonType.Sequence));

        var result = creation.GetMacroReferences();

        Assert.Empty(result);
    }

    [Fact]
    public void GetMacroReferences_ReturnsReference_WithChannelScope_WhenChannelIsNonNegative()
    {
        var creation = CreateCreation(
            CreateControllerAction(deviceId: "device-1", buttonType: ControllerButtonType.Macro, macroId: "macro-1", channel: 0));

        var result = creation.GetMacroReferences();

        var reference = Assert.Single(result);
        Assert.Equal("device-1", reference.DeviceId);
        Assert.Equal("macro-1", reference.MacroId);
        Assert.Equal(MacroScope.Channel, reference.Scope);
    }

    [Fact]
    public void GetMacroReferences_ReturnsReference_WithDeviceScope_WhenChannelIsNegative()
    {
        var creation = CreateCreation(
            CreateControllerAction(deviceId: "device-1", buttonType: ControllerButtonType.Macro, macroId: "macro-1", channel: -1));

        var result = creation.GetMacroReferences();

        var reference = Assert.Single(result);
        Assert.Equal("device-1", reference.DeviceId);
        Assert.Equal("macro-1", reference.MacroId);
        Assert.Equal(MacroScope.Device, reference.Scope);
    }

    [Fact]
    public void GetMacroReferences_ReturnsDistinctReferences_WhenDuplicateMacroActionsExist()
    {
        var creation = CreateCreation(
            CreateControllerAction(deviceId: "device-1", buttonType: ControllerButtonType.Macro, macroId: "macro-1"),
            CreateControllerAction(deviceId: "device-1", buttonType: ControllerButtonType.Macro, macroId: "macro-1"));

        var result = creation.GetMacroReferences();

        Assert.Single(result);
    }

    [Fact]
    public void GetMacroReferences_ReturnsMultipleReferences_WhenDifferentDevicesOrMacrosAreUsed()
    {
        var creation = CreateCreation(
            CreateControllerAction(deviceId: "device-1", buttonType: ControllerButtonType.Macro, macroId: "macro-1"),
            CreateControllerAction(deviceId: "device-1", buttonType: ControllerButtonType.Macro, macroId: "macro-2"),
            CreateControllerAction(deviceId: "device-2", buttonType: ControllerButtonType.Macro, macroId: "macro-1"));

        var result = creation.GetMacroReferences();

        Assert.Equal(3, result.Count);
        Assert.Contains(result, r => r.DeviceId == "device-1" && r.MacroId == "macro-1");
        Assert.Contains(result, r => r.DeviceId == "device-1" && r.MacroId == "macro-2");
        Assert.Contains(result, r => r.DeviceId == "device-2" && r.MacroId == "macro-1");
    }

    private static ControllerAction CreateControllerAction(
    string deviceId = "device-1",
    ControllerButtonType buttonType = ControllerButtonType.Normal,
    string macroId = "",
    int channel = 0)
    {
        return new ControllerAction
        {
            DeviceId = deviceId,
            ButtonType = buttonType,
            MacroId = macroId,
            Channel = channel
        };
    }

    private static Creation CreateCreation(params ControllerAction[] controllerActions)
    {
        var controllerEvent = new ControllerEvent();
        foreach (var controllerAction in controllerActions)
        {
            controllerEvent.ControllerActions.Add(controllerAction);
        }

        var controllerProfile = new ControllerProfile();
        controllerProfile.ControllerEvents.Add(controllerEvent);

        var creation = new Creation();
        creation.ControllerProfiles.Add(controllerProfile);

        return creation;
    }
}
