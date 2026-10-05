using System;
using System.Collections.Generic;
using BrickController2.BusinessLogic;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.InputDeviceManagement;
using BrickController2.PlatformServices.InputDevice;
using Moq;
using Xunit;

namespace BrickController2.Tests.BusinessLogic;

public class CreationPlaySessionTests
{
    private sealed class Input(string runtime, string? descriptor) : IInputDevice
    {
        public string RuntimeId => runtime;
        public string? AssignmentId => descriptor;
        public string InputDeviceId => "Controller 1";
        public int InputDeviceNumber => 1;
        public string Name => "Generic gamepad";
        public void Start() { }
        public void Stop() { }
    }

    private static Creation Creation(string? assignment, string motor)
    {
        var creation = new Creation { Name = motor, ControllerAssignmentId = assignment };
        var profile = new ControllerProfile();
        foreach (var type in new[] { InputDeviceEventType.Axis, InputDeviceEventType.Button })
        {
            var e = new ControllerEvent { EventType = type, EventCode = type == InputDeviceEventType.Axis ? "X" : "A",
                ControllerId = "Controller 1" };
            e.ControllerActions.Add(new ControllerAction { DeviceId = motor, Channel = 0,
                MaxOutputPercent = 100, AxisActiveZonePercent = 100 });
            profile.ControllerEvents.Add(e);
        }
        creation.ControllerProfiles.Add(profile);
        return creation;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("none")]
    [InlineData("missing")]
    public void NoSpecificDeviceMatches(string? id) => Assert.Null(CreationPlaySession.Resolve(id, [new Input("r1", "d1")]));

    [Fact]
    public void DuplicateDescriptorsRequireAnExplicitConnection()
    {
        IInputDevice[] devices = [new Input("r1", "same"), new Input("r2", "same")];
        Assert.Null(CreationPlaySession.Resolve("same", devices));
        Assert.Equal("r2", CreationPlaySession.Resolve("r2", devices));
    }

    [Fact]
    public void ReconnectUsesDescriptorRatherThanOldRuntimeId()
    {
        Assert.Equal("r1", CreationPlaySession.Resolve("d1", [new Input("r1", "d1")]));
        Assert.Equal("r9", CreationPlaySession.Resolve("d1", [new Input("r9", "d1")]));
        Assert.Null(CreationPlaySession.Resolve("r1", [new Input("r9", "d1")]));
    }

    [Fact]
    public void TwoPlayersRouteInterleavedAxesAndButtonsAndDisconnectIndependently()
    {
        var inputs = new InputDeviceManagerService();
        var motorA = new Mock<Device>("A", "A", Mock.Of<IDeviceRepository>());
        var motorB = new Mock<Device>("B", "B", Mock.Of<IDeviceRepository>());
        var devices = new Mock<IDeviceManager>();
        devices.Setup(d => d.GetDeviceById("A")).Returns(motorA.Object);
        devices.Setup(d => d.GetDeviceById("B")).Returns(motorB.Object);
        IPlayLogic Player() => new PlayLogic(Mock.Of<ICreationManager>(), devices.Object, Mock.Of<ISequencePlayer>());
        var a = Creation("d1", "A");
        var b = Creation("d2", "B");
        var session = new CreationPlaySession(inputs, Player);
        session.Configure([a, b], a, Player());
        session.Listen();
        inputs.AddInputDevice(new Input("r1", "d1"));
        inputs.AddInputDevice(new Input("r2", "d2"));
        session.Start();
        motorA.Invocations.Clear();
        motorB.Invocations.Clear();

        void Send(string runtime, InputDeviceEventType type, float value) => inputs.RaiseEvent(
            new InputDeviceEventArgs("Controller 99", type, type == InputDeviceEventType.Axis ? "X" : "A", value) { RuntimeId = runtime });
        Send("r1", InputDeviceEventType.Axis, 0.5f);
        Send("r2", InputDeviceEventType.Axis, -0.75f);
        motorA.Verify(d => d.SetOutput(0, 0.5f), Times.Once);
        motorA.Verify(d => d.SetOutput(0, -0.75f), Times.Never);
        motorB.Verify(d => d.SetOutput(0, -0.75f), Times.Once);
        motorB.Verify(d => d.SetOutput(0, 0.5f), Times.Never);
        Send("unassigned", InputDeviceEventType.Button, 1);
        motorA.Verify(d => d.SetOutput(0, 1), Times.Never);
        motorB.Verify(d => d.SetOutput(0, 1), Times.Never);
        Send("r2", InputDeviceEventType.Button, 1);
        motorB.Verify(d => d.SetOutput(0, 1), Times.Once);
        motorA.Verify(d => d.SetOutput(0, 1), Times.Never);

        inputs.TryRemoveInputDevice<Input>(d => d.RuntimeId == "r1", out _);
        motorA.Verify(d => d.SetOutput(0, 0), Times.Once);
        Send("r2", InputDeviceEventType.Axis, -0.25f);
        motorB.Verify(d => d.SetOutput(0, -0.25f), Times.Once);
        Send("r1", InputDeviceEventType.Button, 1);
        motorA.Verify(d => d.SetOutput(0, 1), Times.Never);
        inputs.AddInputDevice(new Input("r9", "d1"));
        Send("r9", InputDeviceEventType.Button, 1);
        motorA.Verify(d => d.SetOutput(0, 1), Times.Once);
        session.Close();
    }

    [Fact]
    public void AnyControllerRetainsLegacySingleCreationMapping()
    {
        var inputs = new InputDeviceManagerService();
        var player = new Mock<IPlayLogic>();
        player.SetupAllProperties();
        var creation = Creation(null, "A");
        var session = new CreationPlaySession(inputs, () => player.Object);
        session.Configure([creation], creation, player.Object);
        session.Listen();
        session.Start();
        var e = new InputDeviceEventArgs("Controller 1", InputDeviceEventType.Button, "A", 1);
        inputs.RaiseEvent(e);
        player.Verify(p => p.ProcessGameControllerEvent(e), Times.Once);
        Assert.False(player.Object.UseCreationControllerAssignment);
        session.Close();
    }

    [Fact]
    public void DuplicateCreationAssignmentsDoNotBroadcast()
    {
        var inputs = new InputDeviceManagerService();
        var a = Creation("d1", "A");
        var b = Creation("d1", "B");
        var players = new List<Mock<IPlayLogic>>();
        IPlayLogic Player()
        {
            var p = new Mock<IPlayLogic>();
            p.SetupAllProperties();
            players.Add(p);
            return p.Object;
        }
        var session = new CreationPlaySession(inputs, Player);
        session.Configure([a, b], a, Player());
        session.Listen();
        inputs.AddInputDevice(new Input("r1", "d1"));
        session.Start();
        inputs.RaiseEvent(new InputDeviceEventArgs("Controller 1", InputDeviceEventType.Button, "A", 1) { RuntimeId = "r1" });
        foreach (var p in players) p.Verify(x => x.ProcessGameControllerEvent(It.IsAny<InputDeviceEventArgs>()), Times.Never);
        session.Close();
    }

    [Fact]
    public void LiveDescriptorCollisionStopsAndStaysPausedUntilReassignment()
    {
        var inputs = new InputDeviceManagerService();
        var player = new Mock<IPlayLogic>();
        player.SetupAllProperties();
        var creation = Creation("d1", "A");
        var session = new CreationPlaySession(inputs, () => player.Object);
        session.Configure([creation], creation, player.Object);
        session.Listen();
        inputs.AddInputDevice(new Input("r1", "d1"));
        session.Start();
        player.Invocations.Clear();
        inputs.AddInputDevice(new Input("r2", "d1"));
        player.Verify(p => p.StopPlay(), Times.Once);
        inputs.TryRemoveInputDevice<Input>(d => d.RuntimeId == "r1", out _);
        inputs.RaiseEvent(new InputDeviceEventArgs("Controller 1", InputDeviceEventType.Button, "A", 1) { RuntimeId = "r2" });
        player.Verify(p => p.ProcessGameControllerEvent(It.IsAny<InputDeviceEventArgs>()), Times.Never);
        session.Close();
    }

    [Fact]
    public void NoneIgnoresInputAndClosingSessionPreventsFurtherEvents()
    {
        var inputs = new InputDeviceManagerService();
        var player = new Mock<IPlayLogic>();
        player.SetupAllProperties();
        var creation = Creation("none", "A");
        var session = new CreationPlaySession(inputs, () => player.Object);
        session.Configure([creation], creation, player.Object);
        session.Listen();
        session.Start();
        var e = new InputDeviceEventArgs("Controller 1", InputDeviceEventType.Button, "A", 1);
        inputs.RaiseEvent(e);
        session.Close();
        inputs.RaiseEvent(e);
        player.Verify(p => p.ProcessGameControllerEvent(It.IsAny<InputDeviceEventArgs>()), Times.Never);
        player.Verify(p => p.StartPlay(), Times.Never);
    }
}
