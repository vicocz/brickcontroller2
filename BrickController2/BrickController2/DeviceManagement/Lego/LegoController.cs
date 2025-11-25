using BrickController2.PlatformServices.InputDevice;
using BrickController2.PlatformServices.InputDeviceService;
using System.Collections.Generic;
using static BrickController2.PlatformServices.InputDevice.InputDevices;

namespace BrickController2.DeviceManagement.Lego;

internal class LegoController : InputDeviceBase<LegoRemoteControl>
{
    public LegoController(IInputDeviceEventServiceInternal service, LegoRemoteControl remoteControl, int controllerNumber)
        : base(service, remoteControl)
    {
        Name = remoteControl.Name;
        InputDeviceNumber = controllerNumber;
        InputDeviceId = GetControllerIdFromNumber(controllerNumber);
    }

    public override void Start()
    {
        base.Start();
        // link LegoRemoteControl and connect
        InputDeviceDevice.LinkLegoController(this);
        _ = InputDeviceDevice.ConnectAsync(false, (d) => { }, [], false, false, default);
    }

    public override void Stop()
    {
        base.Stop();
        _ = InputDeviceDevice.DisconnectAsync();
        // reset LegoRemoteControl link
        InputDeviceDevice.LinkLegoController(default);
    }

    internal void RaiseEvents(Dictionary<(InputDeviceEventType, string), float> events)
        => RaiseEvent(events);
    internal void RaiseButtonEvent(string eventCode, bool pressed)
        => RaiseEvent(InputDeviceEventType.Button, eventCode, pressed ? BUTTON_PRESSED : BUTTON_RELEASED);
}
