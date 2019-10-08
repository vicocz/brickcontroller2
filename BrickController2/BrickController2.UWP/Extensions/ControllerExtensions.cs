using System;
using Windows.Gaming.Input;

namespace BrickController2.Windows.Extensions
{
    public static class ControllerExtensions
    {
        public static float ToControllerValue(this double value)
        {
            if (Math.Abs(value) < 0.05)
            {
                return 0.0F;
            }
            if (value > 0.95)
            {
                return 1.0F;
            }
            if (value < -0.95)
            {
                return -1.0F;
            }
            return (float)value;
        }

        public static string GetDeviceId(this Gamepad gamepad)
        {
            // kinda hack
            return gamepad.User.NonRoamableId;
        }
    }
}
