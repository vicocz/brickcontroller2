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
    }

    public void Dispose()
    {
        InputDeviceDevice.Dispose();
    }

    public override void Start()
    {
        base.Start();

        // initialize
        var gameControllerType = GetGameControllerType(InputDeviceDevice);
        SetupController(InputDeviceDevice, gameControllerType);
    }

    public override void Stop()
    {
        // detach native handlers first, so no callback can reach this connector once the reset events are published
        var gameControllerType = GetGameControllerType(InputDeviceDevice);
        TeardownController(InputDeviceDevice, gameControllerType);

        base.Stop();
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

    private static void TeardownController(GCController gameController, GameControllerType gameControllerType)
    {
        switch (gameControllerType)
        {
            case GameControllerType.Micro:
                TeardownMicroGamePad(gameController.MicroGamepad!);
                break;

            case GameControllerType.Standard:
#pragma warning disable CA1422 // Validate platform compatibility
                TeardownGamePad(gameController.Gamepad!);
#pragma warning restore CA1422 // Validate platform compatibility
                break;

            case GameControllerType.Extended:
                TeardownExtendedGamePad(gameController.ExtendedGamepad!);
                break;
        }
    }

    private static GameControllerType GetGameControllerType(GCController controller)
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

    private static void TeardownMicroGamePad(GCMicroGamepad gamePad)
    {
        TeardownDigitalButtonInput(gamePad.ButtonA);
        TeardownDigitalButtonInput(gamePad.ButtonX);

        TeardownDPadInput(gamePad.Dpad);
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

    private static void TeardownGamePad(GCGamepad gamePad)
    {
#pragma warning disable CA1422 // Validate platform compatibility
        TeardownDigitalButtonInput(gamePad.ButtonA);
        TeardownDigitalButtonInput(gamePad.ButtonB);
        TeardownDigitalButtonInput(gamePad.ButtonX);
        TeardownDigitalButtonInput(gamePad.ButtonY);

        TeardownDigitalButtonInput(gamePad.LeftShoulder);
        TeardownDigitalButtonInput(gamePad.RightShoulder);

        TeardownDPadInput(gamePad.DPad);
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

    private static void TeardownExtendedGamePad(GCExtendedGamepad gamePad)
    {
        TeardownDigitalButtonInput(gamePad.ButtonA);
        TeardownDigitalButtonInput(gamePad.ButtonB);
        TeardownDigitalButtonInput(gamePad.ButtonX);
        TeardownDigitalButtonInput(gamePad.ButtonY);

        TeardownDigitalButtonInput(gamePad.LeftShoulder);
        TeardownDigitalButtonInput(gamePad.RightShoulder);

        TeardownAnalogButtonInput(gamePad.LeftTrigger);
        TeardownAnalogButtonInput(gamePad.RightTrigger);

        TeardownDPadInput(gamePad.DPad);

        TeardownDigitalOptionalButtonInput(gamePad.LeftThumbstickButton);
        TeardownDigitalOptionalButtonInput(gamePad.RightThumbstickButton);

        TeardownJoyInput(gamePad.LeftThumbstick);
        TeardownJoyInput(gamePad.RightThumbstick);
    }

    private void SetupDigitalOptionalButtonInput(GCControllerButtonInput? button, string name)
    {
        if (button is null)
        {
            return;
        }

        SetupDigitalButtonInput(button, name);
    }

    private static void TeardownDigitalOptionalButtonInput(GCControllerButtonInput? button)
    {
        if (button is null)
        {
            return;
        }

        TeardownDigitalButtonInput(button);
    }

    private void SetupDigitalButtonInput(GCControllerButtonInput button, string name)
    {
        button.ValueChangedHandler = (btn, value, isPressed) =>
        {
            this.RaiseButtonEventConditionally(name, isPressed);
        };
    }

    private static void TeardownDigitalButtonInput(GCControllerButtonInput button)
    {
        button.ValueChangedHandler = null;
    }

    private void SetupAnalogButtonInput(GCControllerButtonInput button, string name)
    {
        button.ValueChangedHandler = (btn, value, isPressed) =>
        {
            value = value < 0.1 ? 0.0F : value;

            this.RaiseAxisEventConditionally(name, value);
        };
    }

    private static void TeardownAnalogButtonInput(GCControllerButtonInput button)
    {
        button.ValueChangedHandler = null;
    }

    private void SetupDPadInput(GCControllerDirectionPad dPad, string name)
    {
        SetupDigitalAxisInput(dPad.XAxis, $"{name}_X");
        SetupDigitalAxisInput(dPad.YAxis, $"{name}_Y");
    }

    private static void TeardownDPadInput(GCControllerDirectionPad dPad)
    {
        TeardownDigitalAxisInput(dPad.XAxis);
        TeardownDigitalAxisInput(dPad.YAxis);
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

            this.RaiseAxisEventConditionally(name, value);
        };
    }

    private static void TeardownDigitalAxisInput(GCControllerAxisInput axis)
    {
        axis.ValueChangedHandler = null;
    }

    private void SetupJoyInput(GCControllerDirectionPad joy, string name)
    {
        SetupAnalogAxisInput(joy.XAxis, $"{name}_X");
        SetupAnalogAxisInput(joy.YAxis, $"{name}_Y");
    }

    private static void TeardownJoyInput(GCControllerDirectionPad joy)
    {
        TeardownAnalogAxisInput(joy.XAxis);
        TeardownAnalogAxisInput(joy.YAxis);
    }

    private void SetupAnalogAxisInput(GCControllerAxisInput axis, string name)
    {
        axis.ValueChangedHandler = (ax, value) =>
        {
            value = AdjustControllerValue(value);

            this.RaiseAxisEventConditionally(name, value);
        };
    }

    private static void TeardownAnalogAxisInput(GCControllerAxisInput axis)
    {
        axis.ValueChangedHandler = null;
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