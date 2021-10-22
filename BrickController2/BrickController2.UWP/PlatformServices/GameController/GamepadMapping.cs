using System;
using System.Collections.Generic;
using Windows.System;

namespace BrickController2.Windows.PlatformServices.GameController
{
    public static class GamepadMapping
    {
        public const float Positive = 1.0f;
        public const float Negative = -1.0f;

        public const string XAxis = "X";
        public const string YAxis = "Y";
        public const string RzAxis = "Rz";
        public const string ZAxis = "Z";
        public const string BrakeAxis = "Brake";
        public const string GasAxis = "Gas";

        /// <summary>
        /// Set of virtual keys related to gamepad buttons
        /// </summary>
        private static readonly Dictionary<VirtualKey, (string, float)> GamePadButtonMapping = new Dictionary<VirtualKey, (string, float)>()
        {
            { VirtualKey.GamepadA, ("ButtonA", Positive) },
            { VirtualKey.GamepadB, ("ButtonB", Positive) },
            { VirtualKey.GamepadX, ("ButtonX", Positive) },
            { VirtualKey.GamepadY, ("ButtonY", Positive) },

            { VirtualKey.GamepadLeftShoulder, ("ButtonL1", Positive) },
            { VirtualKey.GamepadRightShoulder, ("ButtonR1", Positive) },
            { VirtualKey.GamepadLeftTrigger, ("ButtonL2", Positive) },
            { VirtualKey.GamepadRightTrigger, ("ButtonR2", Positive) },

            { VirtualKey.GamepadDPadUp, ("HatY", Negative) },
            { VirtualKey.GamepadDPadDown, ("HatY", Positive) },
            { VirtualKey.GamepadDPadLeft, ("HatX", Negative) },
            { VirtualKey.GamepadDPadRight, ("HatX", Positive) },

            { VirtualKey.GamepadMenu, ("ButtonStart", Positive) },
            { VirtualKey.GamepadView, ("ButtonSelect", Positive) },

            { VirtualKey.GamepadLeftThumbstickButton, ("ButtonThumbl", Positive) },
            { VirtualKey.GamepadRightThumbstickButton, ("ButtonThumbr", Positive) },
        };

        /// <summary>
        /// Set of inverted axis
        /// </summary>
        private static readonly HashSet<string> GamePadInvertedAxis = new HashSet<string> { YAxis, RzAxis };

        public static bool IsGamepadButton(VirtualKey virtualKey, out string buttonCode, out float buttonValue)
        {
            if (GamePadButtonMapping.TryGetValue(virtualKey, out (string, float) button))
            {
                buttonCode = button.Item1;
                buttonValue = button.Item2;

                return true;
            }

            buttonCode = default;
            buttonValue = default;

            return false;
        }

        public static (string AxisName, float Value) GetAxisValue(string axisName, double value)
        {
            if (Math.Abs(value) < 0.05)
            {
                return (axisName, 0.0F);
            }

            float coef = GamePadInvertedAxis.Contains(axisName) ? Negative : Positive;

            if (value > 0.95)
            {
                return (axisName, coef);
            }
            if (value < -0.95)
            {
                return (axisName, -coef);
            }
            return (axisName, coef * (float)value);
        }
    }
}
