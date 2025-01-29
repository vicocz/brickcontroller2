using Android.Views;
using BrickController2.Droid.Extensions;
using BrickController2.Helpers;
using BrickController2.PlatformServices.GameController;
using System;
using System.Collections.Generic;

using static BrickController2.PlatformServices.GameController.GameController;

namespace BrickController2.Droid.PlatformServices.GameController
{
    internal class GamepadController
    {
        /// <summary>
        /// reference to GameControllerService (for future usage)
        /// </summary>
        private readonly GameControllerService _controllerService;

        /// <summary>
        /// reference to InputDevice (for future usage i.e. to get more infos)
        /// </summary>
        private readonly InputDevice _gamepad;

        /// <summary>
        /// zero-based Index of this controller inside the controller management
        /// </summary>
        private readonly int _controllerIndex;

        /// <summary>
        /// string to identify the controller like "Controller 1"
        /// </summary>
        private readonly string _controllerId;

        /// <summary>
        /// Unique and persistant identifier of device (for future usage i.e. to save some device specific settings)
        /// this value won't change even if the input device is disconnected, reconnected, or reconfigured
        /// </summary>
        private readonly string _uniquePersistantDeviceId;

        private readonly Dictionary<Axis, float> _lastAxisValues = [];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">reference to GameControllerService</param>
        /// <param name="gamePad"> reference to InputDevice</param>
        /// <param name="controllerIndex">zero-based Index of device inside the controller management</param>
        public GamepadController(GameControllerService service, InputDevice gamePad, int controllerIndex)
        {
            _controllerService = service;
            _gamepad = gamePad;
            _controllerIndex = controllerIndex;
            _uniquePersistantDeviceId = gamePad.GetUniquePersistentDeviceId();
            _controllerId = GameControllerHelper.GetControllerIdFromIndex(controllerIndex);
        }

        /// <summary>
        /// Unique and persistant identifier of device
        /// </summary>
        public string UniquePersistantDeviceId => _uniquePersistantDeviceId;

        /// <summary>
        /// Index of this controller inside the controller management
        /// </summary>
        public int ControllerIndex => _controllerIndex;

        /// <summary>
        /// string to identify the controller like "Controller 1"
        /// </summary>
        public string ControllerId => _controllerId;

        internal Dictionary<(GameControllerEventType, string), float> GetAxisEvents(MotionEvent e)
        {
            var events = new Dictionary<(GameControllerEventType, string), float>();
            foreach (Axis axisCode in Enum.GetValues<Axis>())
            {
                var axisValue = e.GetAxisValue(axisCode);

                if ((axisCode == Axis.Rx || axisCode == Axis.Ry) &&
                    e.Device?.VendorId == 1356 &&
                    (e.Device?.ProductId == 2508 || e.Device?.ProductId == 1476))
                {
                    // DualShock 4 hack for the triggers ([-1:1] -> [0:1])
                    if (!_lastAxisValues.ContainsKey(axisCode) && axisValue == 0.0F)
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

                if (_lastAxisValues.TryGetValue(axisCode, out float lastValue))
                {
                    if (AreAlmostEqual(axisValue, lastValue))
                    {
                        // axisValue == lastValue
                        continue;
                    }
                }

                _lastAxisValues[axisCode] = axisValue;
                events[(GameControllerEventType.Axis, axisCode.ToString())] = axisValue;
            }
            return events;
        }
    }
}