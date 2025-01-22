using Android.Views;
using BrickController2.Droid.Extensions;
using BrickController2.Helpers;

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
    }
}