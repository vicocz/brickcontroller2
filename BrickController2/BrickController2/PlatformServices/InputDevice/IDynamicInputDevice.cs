namespace BrickController2.PlatformServices.InputDevice;

internal interface IDynamicInputDevice
{
    void ConnectInputController(IInputDeviceConnector controller);
    void DisconnectInputController();
}
