using BrickController2.BusinessLogic;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.Macros;
using FluentAssertions;
using Moq;
using System.Collections.ObjectModel;
using Xunit;

namespace BrickController2.Tests.BusinessLogic;

public class PlayLogicTests
{
    private readonly Mock<ICreationManager> _creationManagerMock = new(MockBehavior.Strict);
    private readonly Mock<IDeviceManager> _deviceManagerMock = new(MockBehavior.Strict);
    private readonly Mock<ISequencePlayer> _sequencePlayerMock = new(MockBehavior.Strict);
    private readonly PlayLogic _playLogic;

    public PlayLogicTests()
    {
        _creationManagerMock.SetupGet(cm => cm.Sequences).Returns(new ObservableCollection<Sequence>());

        _playLogic = new PlayLogic(
            _creationManagerMock.Object,
            _deviceManagerMock.Object,
            _sequencePlayerMock.Object);
    }

    private static ControllerAction CreateControllerAction(
        string deviceId = "device-1",
        ControllerButtonType buttonType = ControllerButtonType.Normal,
        string sequenceName = "",
        string macroId = "")
    {
        return new ControllerAction
        {
            DeviceId = deviceId,
            ButtonType = buttonType,
            SequenceName = sequenceName,
            MacroId = macroId
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

        var deviceMock = CreateDeviceMock();
        _deviceManagerMock.Setup(dm => dm.GetDeviceById("device-1")).Returns(deviceMock.Object);

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

        var deviceMock = CreateDeviceMock();
        deviceMock.SetupGet(x => x.AvailableMacros).Returns([]);
        _deviceManagerMock.Setup(dm => dm.GetDeviceById("device-1")).Returns(deviceMock.Object);

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

        var deviceMock = CreateDeviceMock();
        deviceMock.SetupGet(x => x.AvailableMacros).Returns([new MacroDescriptor("macro-1", "name-key", MacroScope.Channel, MacroKind.OneShot)]);
        _deviceManagerMock.Setup(dm => dm.GetDeviceById("device-1")).Returns(deviceMock.Object);

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

        var deviceMock = CreateDeviceMock();
        _deviceManagerMock.Setup(dm => dm.GetDeviceById("device-1")).Returns(deviceMock.Object);

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

        var deviceMock = CreateDeviceMock();
        _deviceManagerMock.Setup(dm => dm.GetDeviceById("device-1")).Returns(deviceMock.Object);

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

        var deviceMock = CreateDeviceMock();
        _deviceManagerMock.Setup(dm => dm.GetDeviceById("device-1")).Returns(deviceMock.Object);

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

        var deviceMock = CreateDeviceMock();
        deviceMock.SetupGet(x => x.AvailableMacros).Returns([]);
        _deviceManagerMock.Setup(dm => dm.GetDeviceById("device-1")).Returns(deviceMock.Object);

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

        var deviceMock = CreateDeviceMock();
        deviceMock.SetupGet(x => x.AvailableMacros).Returns([new MacroDescriptor("macro-1", "name-key", MacroScope.Device, MacroKind.OneShot)]);
        _deviceManagerMock.Setup(dm => dm.GetDeviceById("device-1")).Returns(deviceMock.Object);

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

        var deviceMock = CreateDeviceMock();
        deviceMock.SetupGet(x => x.AvailableMacros).Returns([new MacroDescriptor("macro-1", "name-key", MacroScope.Channel, MacroKind.OneShot)]);
        _deviceManagerMock.Setup(dm => dm.GetDeviceById("device-1")).Returns(deviceMock.Object);

        var result = _playLogic.ValidateControllerAction(controllerAction);

        result.Should().BeTrue();
    }

    private static Mock<Device> CreateDeviceMock()
    {
        var deviceMock = new Mock<Device>(
            "FakeDevice",                    // name
            "00:00:00:00:00:00",             // address
            Mock.Of<IDeviceRepository>());   // deviceRepository

        return deviceMock;
    }
}
