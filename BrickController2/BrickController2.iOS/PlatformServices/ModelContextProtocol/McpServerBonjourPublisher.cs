using Foundation;

namespace BrickController2.iOS.PlatformServices.ModelContextProtocol;

public class McpServerBonjourPublisher
{
    private NSNetService? _netService = null;

    public void Publish(int port)
    {
#pragma warning disable CA1422 // Validate platform compatibility
        _netService = new NSNetService("local.", "_http._tcp", "Brickcontroller2 MCP Server", port);
        _netService.Publish();
#pragma warning restore CA1422 // Validate platform compatibility
    }

    public void Stop()
    {
#pragma warning disable CA1422 // Validate platform compatibility
        _netService?.Stop();
#pragma warning restore CA1422 // Validate platform compatibility
    }
}
