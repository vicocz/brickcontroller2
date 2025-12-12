using BrickController2.PlatformServices.InputDevice;
using BrickController2.PlatformServices.InputDeviceService;
using System.Collections.Generic;
using System.Linq;

using static BrickController2.PlatformServices.InputDevice.InputDevices;

namespace BrickController2.DeviceManagement.Lego;

internal class LegoRemoteController : InputDeviceBase<RemoteControl>
{
    public LegoRemoteController(IInputDeviceEventServiceInternal service, RemoteControl remoteControl, int controllerNumber)
        : base(service, remoteControl)
    {
        Name = remoteControl.Name;
        InputDeviceNumber = controllerNumber;
        InputDeviceId = $"Controller ({remoteControl.Address})";
    }

    public override void Start()
    {
        base.Start();
        // link Lego RemoteControl and connect
        InputDeviceDevice.LinkLegoController(this);
        _ = InputDeviceDevice.ConnectAsync(false,
            (d) =>
            {
                // reset events on random disconnection
                InputDeviceDevice?.ResetEvents();
            },
            channelConfigurations: [],
            startOutputProcessing: false,
            requestDeviceInformation: false,
            token: default);
    }

    public override void Stop()
    {
        base.Stop();
        _ = InputDeviceDevice.DisconnectAsync();
        // reset Lego RemoteControl link
        InputDeviceDevice.LinkLegoController(default);
    }

    internal void RaiseButtonEvents(IEnumerable<(string eventName, bool pressed)> buttonEvents)
    {
        var events = buttonEvents
            .Where(e => HasValueChanged(e.eventName, GetButtonValue(e)))
            .ToDictionary(e => (InputDeviceEventType.Button, e.eventName), GetButtonValue);

        if (events.Count == 0)
        {
            return;
        }

        RaiseEvent(events);
    }

    private static float GetButtonValue((string eventName, bool pressed) e) => e.pressed ? BUTTON_PRESSED : BUTTON_RELEASED;
}
