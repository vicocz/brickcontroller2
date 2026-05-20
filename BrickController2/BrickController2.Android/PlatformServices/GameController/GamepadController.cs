using Android.Views;
using BrickController2.PlatformServices.InputDevice;
using BrickController2.PlatformServices.InputDeviceService;
using System;
using System.Collections.Generic;

using static BrickController2.PlatformServices.InputDevice.InputDevices;

namespace BrickController2.Droid.PlatformServices.GameController
{
    internal class GamepadController : InputDeviceBase<InputDevice>
    {
        /// <summary>
        /// Set of supported axes (might get filtered in future)
        /// </summary>
        private static readonly IReadOnlyCollection<Axis> SupportedAxes = Enum.GetValues<Axis>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">reference to GameControllerService</param>
        /// <param name="gamePad">reference to InputDevice</param>
        public GamepadController(IInputDeviceEventServiceInternal service, InputDevice gamePad)
            : base(service, gamePad)
        {
            // initialize properties
            Name = GetDisplayName(gamePad);
            InputDeviceNumber = gamePad.ControllerNumber;
            InputDeviceId = GetControllerIdFromNumber(gamePad.ControllerNumber);
        }

        internal bool OnButtonEvent(KeyEvent e, float buttonValue)
        {
            // do simple event name mapping
            var eventName = e.KeyCode.ToString();
            RaiseEvent(InputDeviceEventType.Button, eventName, buttonValue);
            return true;
        }

        internal Dictionary<(InputDeviceEventType, string), float> GetAxisEvents(MotionEvent e)
        {
            var events = new Dictionary<(InputDeviceEventType, string), float>();
            foreach (Axis axisCode in SupportedAxes)
            {
                var axisName = axisCode.ToString();
                var axisValue = e.GetAxisValue(axisCode);

                if ((axisCode == Axis.Rx || axisCode == Axis.Ry) &&
                    e.Device?.VendorId == 1356 &&
                    (e.Device?.ProductId == 2508 || e.Device?.ProductId == 1476))
                {
                    // DualShock 4 hack for the triggers ([-1:1] -> [0:1])
                    if (!ContainsAxisValue(axisName) && axisValue == 0.0F)
                    {
                        continue;
                    }

                    axisValue = (axisValue + 1) / 2;
                }

                if (e.Device?.VendorId == 0x057e &&
                    (/*e.Device.ProductId == 0x2006 || e.Device.ProductId == 0x2007 ||*/ e.Device.ProductId == 0x2009))
                {
                    // Nintendo Switch Pro controller hack ([-0.69:0.7] -> [-1:1])
                    // 2006 and 2007 are for the Nintendo Joy-Con controller (haven't reported issues with it)
                    axisValue = Math.Min(1, Math.Max(-1, axisValue / 0.69F));
                }

                if (e.Device?.VendorId == 1118 && e.Device?.ProductId == 765 &&
                    axisCode == Axis.Generic1)
                {
                    // XBox One controller reports a constant value on Generic 1 - filter it out
                    continue;
                }

                axisValue = AdjustControllerValue(axisValue);

                // skip axis if values has not changed (or change is less than 0.001)
                if (!HasValueChanged(axisName, axisValue))
                {
                    continue;
                }
                events[(InputDeviceEventType.Axis, axisName)] = axisValue;
            }
            return events;
        }

        internal bool OnAxisEvent(MotionEvent e)
        {
            // grab all changed axis event
            var events = GetAxisEvents(e);
            RaiseEvent(events);

            return true;
        }

        private static string GetDisplayName(InputDevice device)
        {
            // Try Name first
            if (!string.IsNullOrWhiteSpace(device.Name))
                return device.Name;

            var deviceType = device.Sources switch
            {
                var s when s.HasFlag(InputSourceType.Gamepad) => "Gamepad",
                var s when s.HasFlag(InputSourceType.Joystick) => "Joystick",
                var s when s.HasFlag(InputSourceType.Dpad) => "DPad",
                _ => "Device"
            };

            // apply some fallbacks using VendorId / ProductId if possible
            if (device.VendorId != 0 || device.ProductId != 0)
                return $"{deviceType} ({device.VendorId:X4}:{device.ProductId:X4})";

            return $"{deviceType}";
        }
    }
}