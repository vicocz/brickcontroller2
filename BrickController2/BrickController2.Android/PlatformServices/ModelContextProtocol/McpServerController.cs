using BrickController2.Droid.PlatformServices.GameController;
using BrickController2.PlatformServices.GameController;
using BrickController2.PlatformServices.ModelContextProtocol;
using System.Collections.Generic;
using System.Linq;

namespace BrickController2.Droid.PlatformServices.ModelContextProtocol
{
    internal class McpServerController : GamepadControllerBase<McpServer>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">reference to GameControllerService</param>
        /// <param name="mcpServer">reference to InputDevice</param>
        public McpServerController(GameControllerService service, McpServer mcpServer)
            : base(service, mcpServer)
        {
            ControllerDevice.ChannelStatesUpdated += mcpServer_ChannelStatesUpdated;

            // initialize properties
            Name = "McpServer";
            ControllerNumber = -1;
            ControllerId = "McpServer";
        }

        private void mcpServer_ChannelStatesUpdated(List<McpServer.ChannelValue> obj)
        {
            // grab all changed axis event
            var currentEvents = obj
                .Where(x => HasValueChanged($"{x.Channel}", (float)x.Value))
                .ToDictionary(x => (GameControllerEventType.Axis, $"{x.Channel}"), x => (float)x.Value);

            RaiseEvent(currentEvents);
        }
    }
}