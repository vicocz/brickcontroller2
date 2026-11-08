using BrickController2.BusinessLogic;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.Macros;
using BrickController2.PlatformServices.InputDevice;
using FluentAssertions;
using Moq;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using Xunit;

namespace BrickController2.Tests.BusinessLogic;

public class PlayLogicTests
{
    private const string DeviceId = "device-1";
    private const string ControllerId = "controller-1";
    private const string EventCode = "A";

    private readonly Mock<ICreationManager> _creationManagerMock = new(MockBehavior.Strict);
    private readonly Mock<IDeviceManager> _deviceManagerMock = new(MockBehavior.Strict);
    private readonly Mock<Device> _deviceMock;
    private readonly Mock<ISequencePlayer> _sequencePlayerMock = new(MockBehavior.Strict);
    private readonly PlayLogic _playLogic;

    public PlayLogicTests()
    {
        _creationManagerMock.SetupGet(cm => cm.Sequences).Returns(new ObservableCollection<Sequence>());
        _deviceMock = new Mock<Device>("FakeDevice", "00:00:00:00:00:00", Mock.Of<IDeviceRepository>());
        _deviceManagerMock.Setup(dm => dm.GetDeviceById(DeviceId)).Returns(_deviceMock.Object);

        _playLogic = new PlayLogic(
            _creationManagerMock.Object,
            _deviceManagerMock.Object,
            _sequencePlayerMock.Object);
    }

    [Fact]
    public void ValidateCreation_ReturnsMissingControllerAction_WhenCreationHasNoControllerActions()
    {
        var creation = new Creation();
        creation.ControllerProfiles.Add(new ControllerProfile());

        var result = _playLogic.ValidateCreation(creation);

        result.Should().Be(CreationValidationResult.MissingControllerAction);
    }

    [Fact]
    public void ValidateCreation_ReturnsMissingDevice_WhenDeviceDoesNotExist()
    {
        var creation = CreateCreation(CreateControllerAction(deviceId: "missing-device"));

        _deviceManagerMock.Setup(dm => dm.GetDeviceById("missing-device")).Returns((Device?)null);

        var result = _playLogic.ValidateCreation(creation);

        result.Should().Be(CreationValidationResult.MissingDevice);
    }

    [Fact]
    public void ValidateCreation_ReturnsMissingSequence_WhenSequenceDoesNotExist()
    {
        var creation = CreateCreation(CreateControllerAction(
            deviceId: "device-1",
            buttonType: ControllerButtonType.Sequence,
            sequenceName: "missing-sequence"));

        var result = _playLogic.ValidateCreation(creation);

        result.Should().Be(CreationValidationResult.MissingSequence);
    }

    [Fact]
    public void ValidateCreation_ReturnsMissingMacro_WhenMacroDoesNotExist()
    {
        var creation = CreateCreation(CreateControllerAction(
            deviceId: "device-1",
            buttonType: ControllerButtonType.Macro,
            macroId: "missing-macro"));

        _deviceMock.SetupGet(x => x.SupportsMacros).Returns(true);
        _deviceMock.SetupGet(x => x.AvailableMacros).Returns([]);

        var result = _playLogic.ValidateCreation(creation);

        result.Should().Be(CreationValidationResult.MissingMacro);
    }

    [Fact]
    public void ValidateCreation_ReturnsMissingMacro_WhenDeviceDoesNotSupportMacros()
    {
        var creation = CreateCreation(CreateControllerAction(
            deviceId: "device-1",
            buttonType: ControllerButtonType.Macro,
            macroId: "missing-macro"));

        _deviceMock.SetupGet(x => x.SupportsMacros).Returns(false);

        var result = _playLogic.ValidateCreation(creation);

        result.Should().Be(CreationValidationResult.MissingMacro);
    }

    [Fact]
    public void ValidateCreation_ReturnsOk_WhenCreationIsValid()
    {
        var creation = CreateCreation(
            CreateControllerAction(deviceId: "device-1"),
            CreateControllerAction(deviceId: "device-1", buttonType: ControllerButtonType.Sequence, sequenceName: "seq-1"),
            CreateControllerAction(deviceId: "device-1", buttonType: ControllerButtonType.Macro, macroId: "macro-1"));

        _deviceMock.SetupGet(x => x.SupportsMacros).Returns(true);
        _deviceMock.SetupGet(x => x.AvailableMacros).Returns([new MacroDescriptor("macro-1", "name-key", MacroScope.Channel, MacroKind.OneShot)]);

        _creationManagerMock.SetupGet(cm => cm.Sequences).Returns([new() { Name = "seq-1" }]);

        var result = _playLogic.ValidateCreation(creation);

        result.Should().Be(CreationValidationResult.Ok);
    }

    [Fact]
    public void ValidateControllerAction_ReturnsFalse_WhenDeviceDoesNotExist()
    {
        var controllerAction = CreateControllerAction(deviceId: "missing-device");

        _deviceManagerMock.Setup(dm => dm.GetDeviceById("missing-device")).Returns((Device?)null);

        var result = _playLogic.ValidateControllerAction(controllerAction);

        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateControllerAction_ReturnsTrue_WhenButtonTypeIsNormal()
    {
        var controllerAction = CreateControllerAction(deviceId: "device-1", buttonType: ControllerButtonType.Normal);

        var result = _playLogic.ValidateControllerAction(controllerAction);

        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateControllerAction_ReturnsFalse_WhenSequenceDoesNotExist()
    {
        var controllerAction = CreateControllerAction(
            deviceId: "device-1",
            buttonType: ControllerButtonType.Sequence,
            sequenceName: "missing-sequence");

        var result = _playLogic.ValidateControllerAction(controllerAction);

        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateControllerAction_ReturnsTrue_WhenSequenceExists()
    {
        var controllerAction = CreateControllerAction(
            deviceId: "device-1",
            buttonType: ControllerButtonType.Sequence,
            sequenceName: "seq-1");

        _creationManagerMock.SetupGet(cm => cm.Sequences).Returns([new() { Name = "seq-1" }]);

        var result = _playLogic.ValidateControllerAction(controllerAction);

        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateControllerAction_ReturnsFalse_WhenMacroDoesNotExist()
    {
        var controllerAction = CreateControllerAction(
            deviceId: "device-1",
            buttonType: ControllerButtonType.Macro,
            macroId: "missing-macro");

        _deviceMock.SetupGet(x => x.AvailableMacros).Returns([]);

        var result = _playLogic.ValidateControllerAction(controllerAction);

        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateControllerAction_ReturnsFalse_WhenMacroExistsButScopeIsNotChannel()
    {
        var controllerAction = CreateControllerAction(
            deviceId: "device-1",
            buttonType: ControllerButtonType.Macro,
            macroId: "macro-1");

        _deviceMock.SetupGet(x => x.AvailableMacros).Returns([new MacroDescriptor("macro-1", "name-key", MacroScope.Device, MacroKind.OneShot)]);

        var result = _playLogic.ValidateControllerAction(controllerAction);

        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateControllerAction_ReturnsTrue_WhenMacroExistsWithChannelScope()
    {
        var controllerAction = CreateControllerAction(
            deviceId: "device-1",
            buttonType: ControllerButtonType.Macro,
            macroId: "macro-1");

        _deviceMock.SetupGet(x => x.AvailableMacros).Returns([new MacroDescriptor("macro-1", "name-key", MacroScope.Channel, MacroKind.OneShot)]);

        var result = _playLogic.ValidateControllerAction(controllerAction);

        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessGameControllerEvent_DoesNothing_WhenNoActiveProfile()
    {
        _playLogic.ActiveProfile = null;

        _playLogic.ProcessGameControllerEvent(ButtonEvent(1f));

        _deviceMock.Verify(d => d.SetOutput(It.IsAny<int>(), It.IsAny<float>()), Times.Never);
    }

    [Fact]
    public void ProcessGameControllerEvent_SetsOutput_WhenNormalButtonPressedAndReleased()
    {
        _playLogic.ActiveProfile = CreateProfile(CreateEvent(CreateControllerAction(channel: 2)));

        _playLogic.ProcessGameControllerEvent(ButtonEvent(1f));
        _playLogic.ProcessGameControllerEvent(ButtonEvent(0f));

        _deviceMock.Verify(d => d.SetOutput(2, 1f), Times.Once);
        _deviceMock.Verify(d => d.SetOutput(2, 0f), Times.Once);
    }

    [Fact]
    public void ProcessGameControllerEvent_SetsNegativeOutput_WhenActionIsInverted()
    {
        _playLogic.ActiveProfile = CreateProfile(CreateEvent(CreateControllerAction(channel: 0, isInvert: true)));

        _playLogic.ProcessGameControllerEvent(ButtonEvent(1f));

        _deviceMock.Verify(d => d.SetOutput(0, -1f), Times.Once);
    }

    [Fact]
    public void ProcessGameControllerEvent_TreatsValueBelowThresholdAsReleased()
    {
        _playLogic.ActiveProfile = CreateProfile(CreateEvent(CreateControllerAction(channel: 0)));

        _playLogic.ProcessGameControllerEvent(ButtonEvent(InputDevices.BUTTON_PRESSED_THRESHOLD));

        _deviceMock.Verify(d => d.SetOutput(0, 0f), Times.Once);
    }

    [Fact]
    public void ProcessGameControllerEvent_IgnoresEvent_WhenEventCodeDiffers()
    {
        _playLogic.ActiveProfile = CreateProfile(CreateEvent(CreateControllerAction()));

        _playLogic.ProcessGameControllerEvent(new InputDeviceEventArgs(ControllerId, InputDeviceEventType.Button, "B", 1f));

        _deviceMock.Verify(d => d.SetOutput(It.IsAny<int>(), It.IsAny<float>()), Times.Never);
    }

    [Fact]
    public void ProcessGameControllerEvent_IgnoresEvent_WhenEventTypeDiffers()
    {
        _playLogic.ActiveProfile = CreateProfile(CreateEvent(CreateControllerAction()));

        _playLogic.ProcessGameControllerEvent(new InputDeviceEventArgs(ControllerId, InputDeviceEventType.Axis, EventCode, 1f));

        _deviceMock.Verify(d => d.SetOutput(It.IsAny<int>(), It.IsAny<float>()), Times.Never);
    }

    [Fact]
    public void ProcessGameControllerEvent_IgnoresEvent_WhenControllerIdDiffers()
    {
        _playLogic.ActiveProfile = CreateProfile(CreateEvent(CreateControllerAction()));

        _playLogic.ProcessGameControllerEvent(new InputDeviceEventArgs("other", InputDeviceEventType.Button, EventCode, 1f));

        _deviceMock.Verify(d => d.SetOutput(It.IsAny<int>(), It.IsAny<float>()), Times.Never);
    }

    [Fact]
    public void ProcessGameControllerEvent_MatchesAnyController_WhenEventControllerIdIsEmpty()
    {
        _playLogic.ActiveProfile = CreateProfile(CreateEvent(CreateControllerAction(channel: 1), controllerId: string.Empty));

        _playLogic.ProcessGameControllerEvent(new InputDeviceEventArgs("any", InputDeviceEventType.Button, EventCode, 1f));

        _deviceMock.Verify(d => d.SetOutput(1, 1f), Times.Once);
    }

    [Fact]
    public void ProcessGameControllerEvent_SkipsAction_WhenDeviceIsNotFound()
    {
        _deviceManagerMock.Setup(dm => dm.GetDeviceById("missing")).Returns((Device?)null);
        _playLogic.ActiveProfile = CreateProfile(CreateEvent(
            CreateControllerAction(deviceId: "missing"),
            CreateControllerAction(channel: 3)));

        _playLogic.ProcessGameControllerEvent(ButtonEvent(1f));

        _deviceMock.Verify(d => d.SetOutput(3, 1f), Times.Once);
        _deviceMock.Verify(d => d.SetOutput(It.IsAny<int>(), It.IsAny<float>()), Times.Once);
    }

    [Fact]
    public void ProcessGameControllerEvent_SetsOutputForAxisValue()
    {
        _playLogic.ActiveProfile = CreateProfile(CreateEvent(CreateControllerAction(channel: 0), InputDeviceEventType.Axis));

        _playLogic.ProcessGameControllerEvent(new InputDeviceEventArgs(ControllerId, InputDeviceEventType.Axis, EventCode, 0.5f));

        _deviceMock.Verify(d => d.SetOutput(0, 0.5f), Times.Once);
    }

    [Fact]
    public void ProcessGameControllerEvent_InvokesMacro_WhenMacroButtonPressed()
    {
        SetupMacroDevice(MacroScope.Device);
        var action = CreateControllerAction(channel: ControllerAction.NoChannel, buttonType: ControllerButtonType.Macro, macroId: "macro-1", macroChoice: "sound1.mp3");
        _playLogic.ActiveProfile = CreateProfile(CreateEvent(action));

        _playLogic.ProcessGameControllerEvent(ButtonEvent(1f));

        _deviceMock.Verify(d => d.ExecuteMacroAsync(
            It.Is<MacroInvocation>(i => i.DescriptorId == "macro-1" && i.Channel == null && i.ChoiceValue.ValueEquals("sound1.mp3")),
            It.IsAny<CancellationToken>()), Times.Once);
        _deviceMock.Verify(d => d.SetOutput(It.IsAny<int>(), It.IsAny<float>()), Times.Never);
    }

    [Fact]
    public void ProcessGameControllerEvent_InvokesChannelMacroWithChannel()
    {
        SetupMacroDevice(MacroScope.Channel);
        var action = CreateControllerAction(channel: 2, buttonType: ControllerButtonType.Macro, macroId: "macro-1");
        _playLogic.ActiveProfile = CreateProfile(CreateEvent(action));

        _playLogic.ProcessGameControllerEvent(ButtonEvent(1f));

        _deviceMock.Verify(d => d.ExecuteMacroAsync(
            It.Is<MacroInvocation>(i => i.DescriptorId == "macro-1" && i.Channel == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void ProcessGameControllerEvent_DoesNotInvokeMacro_WhenMacroButtonReleased()
    {
        SetupMacroDevice(MacroScope.Device);
        var action = CreateControllerAction(channel: ControllerAction.NoChannel, buttonType: ControllerButtonType.Macro, macroId: "macro-1");
        _playLogic.ActiveProfile = CreateProfile(CreateEvent(action));

        _playLogic.ProcessGameControllerEvent(ButtonEvent(0f));

        _deviceMock.Verify(d => d.ExecuteMacroAsync(It.IsAny<MacroInvocation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void ProcessGameControllerEvent_DoesNotInvokeMacro_WhenScopeDoesNotMatch()
    {
        SetupMacroDevice(MacroScope.Channel);
        var action = CreateControllerAction(channel: ControllerAction.NoChannel, buttonType: ControllerButtonType.Macro, macroId: "macro-1");
        _playLogic.ActiveProfile = CreateProfile(CreateEvent(action));

        _playLogic.ProcessGameControllerEvent(ButtonEvent(1f));

        _deviceMock.Verify(d => d.ExecuteMacroAsync(It.IsAny<MacroInvocation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void ProcessGameControllerEvent_DoesNotThrow_WhenMacroExecutionFails()
    {
        SetupMacroDevice(MacroScope.Device, throws: true);
        var action = CreateControllerAction(channel: ControllerAction.NoChannel, buttonType: ControllerButtonType.Macro, macroId: "macro-1");
        _playLogic.ActiveProfile = CreateProfile(CreateEvent(action));

        var exception = Record.Exception(() => _playLogic.ProcessGameControllerEvent(ButtonEvent(1f)));

        Assert.Null(exception);
        _deviceMock.Verify(d => d.ExecuteMacroAsync(It.IsAny<MacroInvocation>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetupMacroDevice(MacroScope scope, bool throws = false)
    {
        _deviceMock.SetupGet(d => d.AvailableMacros)
            .Returns([new MacroDescriptor("macro-1", "name-key", scope, MacroKind.OneShot)]);

        var setup = _deviceMock.Setup(d => d.ExecuteMacroAsync(It.IsAny<MacroInvocation>(), It.IsAny<CancellationToken>()));
        if (throws)
        {
            setup.ThrowsAsync(new InvalidOperationException());
        }
        else
        {
            setup.ReturnsAsync(true);
        }
    }

    [Fact]
    public void ProcessGameControllerEvent_StartsSequence_WhenAxisReachesHalfOfRange()
    {
        var sequence = SetupAxisSequenceAction();

        _playLogic.ProcessGameControllerEvent(AxisEvent(0.5f));

        _sequencePlayerMock.Verify(sp => sp.StartSequence(DeviceId, 3, false, sequence), Times.Once);
        _deviceMock.Verify(d => d.SetOutput(It.IsAny<int>(), It.IsAny<float>()), Times.Never);
    }

    [Fact]
    public void ProcessGameControllerEvent_DoesNotStartSequence_WhenAxisIsBelowHalfOfRange()
    {
        SetupAxisSequenceAction();

        _playLogic.ProcessGameControllerEvent(AxisEvent(0.4f));

        _sequencePlayerMock.Verify(sp => sp.StartSequence(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Sequence>()), Times.Never);
    }

    [Fact]
    public void ProcessGameControllerEvent_StopsSequence_WhenAxisFallsBelowHalfOfRange()
    {
        var sequence = SetupAxisSequenceAction();

        _playLogic.ProcessGameControllerEvent(AxisEvent(0.8f));
        _playLogic.ProcessGameControllerEvent(AxisEvent(0.2f));

        _sequencePlayerMock.Verify(sp => sp.StartSequence(DeviceId, 3, false, sequence), Times.Once);
        _sequencePlayerMock.Verify(sp => sp.StopSequence(DeviceId, 3), Times.Once);
    }

    [Fact]
    public void ProcessGameControllerEvent_StartsSequenceOnce_WhenAxisStaysAboveHalfOfRange()
    {
        var sequence = SetupAxisSequenceAction();

        _playLogic.ProcessGameControllerEvent(AxisEvent(0.6f));
        _playLogic.ProcessGameControllerEvent(AxisEvent(0.9f));

        _sequencePlayerMock.Verify(sp => sp.StartSequence(DeviceId, 3, false, sequence), Times.Once);
        _sequencePlayerMock.Verify(sp => sp.StopSequence(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void ProcessGameControllerEvent_StartsSequenceOnNegativeSide_WhenActionIsInverted()
    {
        var sequence = SetupAxisSequenceAction(isInvert: true);

        _playLogic.ProcessGameControllerEvent(AxisEvent(0.8f));
        _sequencePlayerMock.Verify(sp => sp.StartSequence(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Sequence>()), Times.Never);

        _playLogic.ProcessGameControllerEvent(AxisEvent(-0.8f));

        _sequencePlayerMock.Verify(sp => sp.StartSequence(DeviceId, 3, false, sequence), Times.Once);
    }

    [Fact]
    public void ProcessGameControllerEvent_UsesRangeAfterDeadZone_WhenSequenceTriggeredByAxis()
    {
        var sequence = SetupAxisSequenceAction(deadZonePercent: 20);

        // (0.55 - 0.2) / 0.8 = 0.4375 -> below half of the range
        _playLogic.ProcessGameControllerEvent(AxisEvent(0.55f));
        _sequencePlayerMock.Verify(sp => sp.StartSequence(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Sequence>()), Times.Never);

        // (0.7 - 0.2) / 0.8 = 0.625 -> above half of the range
        _playLogic.ProcessGameControllerEvent(AxisEvent(0.7f));

        _sequencePlayerMock.Verify(sp => sp.StartSequence(DeviceId, 3, false, sequence), Times.Once);
    }

    private Sequence SetupAxisSequenceAction(bool isInvert = false, int deadZonePercent = 0)
    {
        var sequence = new Sequence { Name = "seq-1" };
        _creationManagerMock.SetupGet(cm => cm.Sequences).Returns([sequence]);
        _sequencePlayerMock.Setup(sp => sp.StartSequence(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Sequence>()));
        _sequencePlayerMock.Setup(sp => sp.StopSequence(It.IsAny<string>(), It.IsAny<int>()));

        var action = CreateControllerAction(channel: 3, isInvert: isInvert, sequenceName: "seq-1");
        action.AxisType = ControllerAxisType.Sequence;
        action.AxisDeadZonePercent = deadZonePercent;
        _playLogic.ActiveProfile = CreateProfile(CreateEvent(action, InputDeviceEventType.Axis));

        return sequence;
    }

    private static InputDeviceEventArgs AxisEvent(float value)
        => new(ControllerId, InputDeviceEventType.Axis, EventCode, value);

    private static InputDeviceEventArgs ButtonEvent(float value)
        => new(ControllerId, InputDeviceEventType.Button, EventCode, value);

    private static ControllerEvent CreateEvent(
        ControllerAction action,
        InputDeviceEventType eventType = InputDeviceEventType.Button,
        string controllerId = ControllerId)
        => CreateEvent(controllerId, eventType, action);

    private static ControllerEvent CreateEvent(params ControllerAction[] actions)
        => CreateEvent(ControllerId, InputDeviceEventType.Button, actions);

    private static ControllerEvent CreateEvent(string controllerId, InputDeviceEventType eventType, params ControllerAction[] actions)
    {
        var controllerEvent = new ControllerEvent
        {
            ControllerId = controllerId,
            EventType = eventType,
            EventCode = EventCode
        };
        foreach (var action in actions)
        {
            controllerEvent.ControllerActions.Add(action);
        }
        return controllerEvent;
    }

    private static ControllerProfile CreateProfile(ControllerEvent controllerEvent)
    {
        var profile = new ControllerProfile();
        profile.ControllerEvents.Add(controllerEvent);
        return profile;
    }

    private static ControllerAction CreateControllerAction(string deviceId = DeviceId,
        int channel = 0,
        bool isInvert = false,
        ControllerButtonType buttonType = ControllerButtonType.Normal,
        string sequenceName = "",
        string macroId = "",
        MacroChoiceValue macroChoice = default) => new()
        {
            DeviceId = deviceId,
            Channel = channel,
            IsInvert = isInvert,
            ButtonType = buttonType,
            MaxOutputPercent = 100,
            AxisActiveZonePercent = 100,
            SequenceName = sequenceName,
            MacroId = macroId,
            MacroChoice = macroChoice
        };

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
