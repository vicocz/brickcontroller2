using BrickController2.PlatformServices.GameController;
using System;
using System.Collections.Generic;
using Windows.Gaming.Input;

using static BrickController2.PlatformServices.GameController.GameControllers;

namespace BrickController2.Windows.Extensions;

internal static class GamepadReadingExtenions
{
    public static IEnumerable<(string Name, GameControllerEventType EventType, float Value)> Enumerate(this GamepadReading readings)
    {
        // native axes
        yield return GetAxis("X", readings.LeftThumbstickX, AXIS_MAX_VALUE);
        yield return GetAxis("Y", readings.LeftThumbstickY, AXIS_MIN_VALUE);
        yield return GetAxis("Brake", readings.LeftTrigger, AXIS_MAX_VALUE);
        yield return GetAxis("Z", readings.RightThumbstickX, AXIS_MAX_VALUE);
        yield return GetAxis("Rz", readings.RightThumbstickY, AXIS_MIN_VALUE);
        yield return GetAxis("Gas", readings.RightTrigger, AXIS_MAX_VALUE);

        // buttons treated as axis
        yield return GetHybridButton(readings, GamepadButtons.DPadDown, GamepadButtons.DPadUp, "HatY");
        yield return GetHybridButton(readings, GamepadButtons.DPadRight, GamepadButtons.DPadLeft, "HatX");

        // get buttons
        yield return GetButton(readings, GamepadButtons.A, "ButtonA");
        yield return GetButton(readings, GamepadButtons.B, "ButtonB");
        yield return GetButton(readings, GamepadButtons.X, "ButtonX");
        yield return GetButton(readings, GamepadButtons.Y, "ButtonY");
        yield return GetButton(readings, GamepadButtons.LeftShoulder, "ButtonL1");
        yield return GetButton(readings, GamepadButtons.RightShoulder, "ButtonR1");
        yield return GetButton(readings, GamepadButtons.Menu, "ButtonStart");
        yield return GetButton(readings, GamepadButtons.View, "ButtonSelect");
        yield return GetButton(readings, GamepadButtons.LeftThumbstick, "ButtonThumbl");
        yield return GetButton(readings, GamepadButtons.RightThumbstick, "ButtonThumbr");

        // TODO Home button - 0x40000000

        //TODO
        // GamepadButtons.Paddle1
        // GamepadButtons.Paddle2
        // GamepadButtons.Paddle3
        // GamepadButtons.Paddle4
    }

    private static (string Name, GameControllerEventType Type, float value) GetHybridButton(this GamepadReading readings, GamepadButtons button, GamepadButtons opositeButton, string name)
    {
        // get primary button
        if (readings.Buttons.HasFlag(button))
        {
            return new(name, GameControllerEventType.Axis, AXIS_MAX_VALUE);
        }
        if (readings.Buttons.HasFlag(opositeButton))
        {
            return new(name, GameControllerEventType.Axis, AXIS_MIN_VALUE);
        }
        return new(name, GameControllerEventType.Axis, AXIS_ZERO_VALUE);
    }

    private static (string Name, GameControllerEventType Type, float value) GetButton(this GamepadReading readings, GamepadButtons button, string name)
    {
        // get primary button
        if (readings.Buttons.HasFlag(button))
        {
            return new(name, GameControllerEventType.Button, BUTTON_PRESSED);
        }
        return new(name, GameControllerEventType.Button, BUTTON_RELEASED);
    }

    private static (string Name, GameControllerEventType Type, float value) GetAxis(string name, double value, float maxValue)
    {
        if (Math.Abs(value) < AXIS_DELTA_VALUE)
        {
            return (name, GameControllerEventType.Axis, AXIS_ZERO_VALUE);
        }
        if (value > 0.95)
        {
            return (name, GameControllerEventType.Axis, maxValue);
        }
        if (value < -0.95)
        {
            return (name, GameControllerEventType.Axis, -maxValue);
        }
        return (name, GameControllerEventType.Axis, maxValue * (float)value);
    }
}
