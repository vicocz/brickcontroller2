using System;
using BrickController2.PlatformServices.InputDevice;
using BrickController2.PlatformServices.InputDeviceService;
using GameController;

using static BrickController2.PlatformServices.InputDevice.InputDevices;

internal class GamepadController : InputDeviceBase<GCController>, IDisposable
{
    private enum GameControllerType
    {
        Unknown,
        Micro,
        Standard,
        Extended
    };

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="service">reference to GameControllerService</param>
    /// <param name="controller">reference to InputDevice</param>
    public GamepadController(IInputDeviceEventServiceInternal service, GCController controller)
        : base(service, controller)
    {
        GameControllerType gameControllerType = GetGameControllerType(controller);

        // initialize properties
        Name = GetDisplayName(controller, gameControllerType);
        InputDeviceNumber = (int)controller.PlayerIndex;
        InputDeviceId = GetControllerIdFromNumber(InputDeviceNumber);

        SetupController(controller, gameControllerType);
    }

    public void Dispose()
    {
        InputDeviceDevice.Dispose();
    }

    private void SetupController(GCController gameController, GameControllerType gameControllerType)
    {
        switch (gameControllerType)
        {
            case GameControllerType.Micro:
                SetupMicroGamePad(gameController.MicroGamepad!);
                break;

            case GameControllerType.Standard:
#pragma warning disable CA1422 // Validate platform compatibility
                SetupGamePad(gameController.Gamepad!);
#pragma warning restore CA1422 // Validate platform compatibility
                break;

            case GameControllerType.Extended:
                SetupExtendedGamePad(gameController.ExtendedGamepad!);
                break;
        }
    }

    private GameControllerType GetGameControllerType(GCController controller)
    {
        try
        {
            if (controller.MicroGamepad is not null)
            {
                return GameControllerType.Micro;
            }
        }
        catch (InvalidCastException) { }

        try
        {
#pragma warning disable CA1422 // Validate platform compatibility
            if (controller.Gamepad is not null)
            {
                return GameControllerType.Standard;
            }
#pragma warning restore CA1422 // Validate platform compatibility
        }
        catch (InvalidCastException) { }

        try
        {
            if (controller.ExtendedGamepad is not null)
            {
                return GameControllerType.Extended;
            }
        }
        catch (InvalidCastException) { }

        return GameControllerType.Unknown;
    }

    private void SetupMicroGamePad(GCMicroGamepad gamePad)
    {
        SetupDigitalButtonInput(gamePad.ButtonA, "Button_A");
        SetupDigitalButtonInput(gamePad.ButtonX, "Button_X");

        SetupDPadInput(gamePad.Dpad, "DPad");
    }

    private void SetupGamePad(GCGamepad gamePad)
    {
#pragma warning disable CA1422 // Validate platform compatibility
        SetupDigitalButtonInput(gamePad.ButtonA, "Button_A");
        SetupDigitalButtonInput(gamePad.ButtonB, "Button_B");
        SetupDigitalButtonInput(gamePad.ButtonX, "Button_X");
        SetupDigitalButtonInput(gamePad.ButtonY, "Button_Y");

        SetupDigitalButtonInput(gamePad.LeftShoulder, "LeftShoulder");
        SetupDigitalButtonInput(gamePad.RightShoulder, "RightShoulder");

        SetupDPadInput(gamePad.DPad, "DPad");
#pragma warning restore CA1422 // Validate platform compatibility
    }

    private void SetupExtendedGamePad(GCExtendedGamepad gamePad)
    {
        SetupDigitalButtonInput(gamePad.ButtonA, "Button_A");
        SetupDigitalButtonInput(gamePad.ButtonB, "Button_B");
        SetupDigitalButtonInput(gamePad.ButtonX, "Button_X");
        SetupDigitalButtonInput(gamePad.ButtonY, "Button_Y");

        SetupDigitalButtonInput(gamePad.LeftShoulder, "LeftShoulder");
        SetupDigitalButtonInput(gamePad.RightShoulder, "RightShoulder");

        SetupAnalogButtonInput(gamePad.LeftTrigger, "LeftTrigger");
        SetupAnalogButtonInput(gamePad.RightTrigger, "RightTrigger");

        SetupDPadInput(gamePad.DPad, "DPad");

        SetupDigitalOptionalButtonInput(gamePad.LeftThumbstickButton, "LeftThumbStick_Button");
        SetupDigitalOptionalButtonInput(gamePad.RightThumbstickButton, "RightThumbStick_Button");

        SetupJoyInput(gamePad.LeftThumbstick, "LeftThumbStick");
        SetupJoyInput(gamePad.RightThumbstick, "RightThumbStick");
    }

    private void SetupDigitalOptionalButtonInput(GCControllerButtonInput? button, string name)
    {
        if (button is null)
        {
            return;
        }

        SetupDigitalButtonInput(button, name);
    }

    private void SetupDigitalButtonInput(GCControllerButtonInput button, string name)
    {
        button.ValueChangedHandler = (btn, value, isPressed) =>
        {
            value = isPressed ? BUTTON_PRESSED : BUTTON_RELEASED;

            if (HasValueChanged(name, value))
            {
                RaiseEvent(InputDeviceEventType.Button, name, value);
            }
        };
    }

    private void SetupAnalogButtonInput(GCControllerButtonInput button, string name)
    {
        button.ValueChangedHandler = (btn, value, isPressed) =>
        {
            value = value < 0.1 ? 0.0F : value;

            if (HasValueChanged(name, value))
            {
                RaiseEvent(InputDeviceEventType.Axis, name, value);
            }
        };
    }

    private void SetupDPadInput(GCControllerDirectionPad dPad, string name)
    {
        SetupDigitalAxisInput(dPad.XAxis, $"{name}_X");
        SetupDigitalAxisInput(dPad.YAxis, $"{name}_Y");
    }

    private void SetupDigitalAxisInput(GCControllerAxisInput axis, string name)
    {
        axis.ValueChangedHandler = (ax, value) =>
        {
            // adjust value
            value = value switch
            {
                < -0.1f => AXIS_MIN_VALUE,
                > 0.1f => AXIS_MAX_VALUE,
                _ => AXIS_ZERO_VALUE
            };

            if (HasValueChanged(name, value))
            {
                RaiseEvent(InputDeviceEventType.Axis, name, value);
            }
        };
    }

    private void SetupJoyInput(GCControllerDirectionPad joy, string name)
    {
        SetupAnalogAxisInput(joy.XAxis, $"{name}_X");
        SetupAnalogAxisInput(joy.YAxis, $"{name}_Y");
    }

    private void SetupAnalogAxisInput(GCControllerAxisInput axis, string name)
    {
        axis.ValueChangedHandler = (ax, value) =>
        {
            value = AdjustControllerValue(value);

            if (HasValueChanged(name, value))
            {
                RaiseEvent(InputDeviceEventType.Axis, name, value);
            }
        };
    }

    private static string GetDisplayName(GCController controller, GameControllerType gameControllerType)
    {
        if (!string.IsNullOrEmpty(controller.VendorName))
        {
            return controller.VendorName;
        }

        return gameControllerType switch
        {
            GameControllerType.Micro => "Micro Gamepad",
            GameControllerType.Standard => "Standard Gamepad",
            GameControllerType.Extended => "Extended Gamepad",
            _ => "Unknown Gamepad",
        };
    }
}