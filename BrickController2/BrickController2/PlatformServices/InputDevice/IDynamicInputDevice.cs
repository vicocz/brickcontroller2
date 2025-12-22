using System.Collections.Generic;

namespace BrickController2.PlatformServices.InputDevice;

public interface IDynamicInputDevice
{
}


public interface IDynamicInputDevice<TDevice> : IDynamicInputDevice
    where TDevice : class
{
    void ConnectInputController(IInputDevice<TDevice> controller);
    void DisconnectInputController();
}