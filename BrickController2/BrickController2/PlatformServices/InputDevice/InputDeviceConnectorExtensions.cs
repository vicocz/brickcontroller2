using System.Collections.Generic;
using static BrickController2.PlatformServices.InputDevice.InputDevices;

namespace BrickController2.PlatformServices.InputDevice;

public static class InputDeviceConnectorExtensions
{
    public static bool RaiseAxisEventConditionally(this IInputDeviceConnector? connector, string axisName, float value)
        => RaiseEventConditionally(connector, InputDeviceEventType.Axis, axisName, value);

    public static bool RaiseButtonEventConditionally(this IInputDeviceConnector? connector, string buttonName, bool isPressed)
    {
        var value = isPressed ? BUTTON_PRESSED : BUTTON_RELEASED;
        return RaiseEventConditionally(connector, InputDeviceEventType.Button, buttonName, value);
    }

    public static bool RaiseEventConditionally(this IInputDeviceConnector? connector,
        InputDeviceEventType eventType,
        string eventCode,
        float value)
    {
        if (connector?.HasValueChanged(eventType, eventCode, value) == true)
        {
            connector.RaiseEvent(new Dictionary<(InputDeviceEventType, string), float>
            {
                { (eventType, eventCode), value }
            });
            return true;
        }
        return false;
    }
}
