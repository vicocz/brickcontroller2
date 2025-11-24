using BrickController2.PlatformServices.InputDevice;
using BrickController2.PlatformServices.InputDeviceService;
namespace BrickController2.DeviceManagement.Lego;

internal class LegoController : InputDeviceBase<LegoRemoteControl>
{
    public LegoController(IInputDeviceEventServiceInternal service, LegoRemoteControl remoteControl, int controllerNumber)
        : base(service, remoteControl)
    {
        Name = remoteControl.Name;
        InputDeviceNumber = controllerNumber;
        InputDeviceId = $"LEGO Controller #{InputDeviceNumber}";
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

    internal bool OnButtonEvent(string button, float buttonValue)
    {
        RaiseEvent(InputDeviceEventType.Button, button, buttonValue);
        return true;
    }
}
