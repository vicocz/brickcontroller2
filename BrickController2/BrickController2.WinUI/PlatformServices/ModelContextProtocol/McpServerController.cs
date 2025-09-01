using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Dispatching;
using BrickController2.PlatformServices.GameController;
using BrickController2.Windows.PlatformServices.GameController;
using BrickController2.PlatformServices.ModelContextProtocol;

namespace BrickController2.Windows.PlatformServices.ModelContextProtocol;

internal class McpServerController : GamepadControllerBase<McpServer>
{
    private readonly IDispatcher? _dispatcher;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="service">reference to GameControllerService</param>
    /// <param name="_mcpServer">reference to UWP's Gamepad</param>
    public McpServerController(GameControllerService service, McpServer mcpServer, IDispatcher? dispatcher)
        : base(service, mcpServer)
    {
        _dispatcher = dispatcher;
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

        _dispatcher?.Dispatch(() => RaiseEvent(currentEvents));
    }
}
