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
        private static readonly Dictionary<VirtualKey, string> GamePadButtonMapping = new Dictionary<VirtualKey, string>()
        {
            { VirtualKey.GamepadA, "ButtonA" },
            { VirtualKey.GamepadB, "ButtonB" },
            { VirtualKey.GamepadX, "ButtonX" },
            { VirtualKey.GamepadY, "ButtonY" },

            { VirtualKey.GamepadLeftShoulder, "ButtonL1" },
            { VirtualKey.GamepadRightShoulder, "ButtonR1" },
            { VirtualKey.GamepadLeftTrigger, "ButtonL2" },
            { VirtualKey.GamepadRightTrigger, "ButtonR2" },

            { VirtualKey.GamepadMenu, "ButtonStart" },
            { VirtualKey.GamepadView, "ButtonSelect" },

            { VirtualKey.GamepadLeftThumbstickButton, "ButtonThumbl" },
            { VirtualKey.GamepadRightThumbstickButton, "ButtonThumbr" },
        };

        /// <summary>
        /// Set of virtual keys related to gamepad buttons handled as AXIS
        /// </summary>
        private static readonly Dictionary<VirtualKey, (string, float)> GamePadAxisMapping = new Dictionary<VirtualKey, (string, float)>()
        {
            { VirtualKey.GamepadDPadUp, ("HatY", Negative) },
            { VirtualKey.GamepadDPadDown, ("HatY", Positive) },
            { VirtualKey.GamepadDPadLeft, ("HatX", Negative) },
            { VirtualKey.GamepadDPadRight, ("HatX", Positive) },
        };

        /// <summary>
        /// Set of inverted axis
        /// </summary>
        private static readonly HashSet<string> GamePadInvertedAxis = new HashSet<string> { YAxis, RzAxis };

        public static bool IsGamepadButton(VirtualKey virtualKey, out string buttonCode)
        {
            if (GamePadButtonMapping.TryGetValue(virtualKey, out buttonCode))
            {
                return true;
            }

            buttonCode = default;
            return false;
        }

        public static bool IsGamepadAxis(VirtualKey virtualKey, out string axisCode, out float axisValue)
        {
            if (GamePadAxisMapping.TryGetValue(virtualKey, out (string, float) button))
            {
                axisCode = button.Item1;
                axisValue = button.Item2;

                return true;
            }

            axisCode = default;
            axisValue = default;

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
