using BrickController2.PlatformServices.InputDevice;
using BrickController2.PlatformServices.InputDeviceService;

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
        InputDeviceDevice.ConnectInputController(this);
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
        InputDeviceDevice.DisconnectInputController();
    }
}
