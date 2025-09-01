using BrickController2.PlatformServices.ModelContextProtocol;
using BrickController2.UI.Services.Preferences;
using System;
using System.Threading.Tasks;

namespace BrickController2.iOS.PlatformServices.ModelContextProtocol;

public class McpServerService : IMcpServerService, IDisposable
{
    private readonly object _Lock = new object();
    private readonly IPreferencesService _preferencesService;
    private readonly McpServerBonjourPublisher _mcpServerBonjourPublisher;
    private McpServer? _server;

    public McpServerService(IPreferencesService preferencesService, McpServerBonjourPublisher mcpServerBonjourPublisher)
    {
        _preferencesService = preferencesService;
        _mcpServerBonjourPublisher = mcpServerBonjourPublisher;
        _server = null;

        ApplyMcpServer();
    }

    public event EventHandler<McpServer>? McpServerAdded;
    public event EventHandler<McpServer>? McpServerRemoved;

    public bool IsMcpServerAvailable => true;
    public McpServer? Server => _server;

    public bool IsMcpServerEnabled
    {
        get => _preferencesService.Get("McpServerEnabled", false);

        set
        {
            if (IsMcpServerEnabled != value)
            {
                _preferencesService.Set("McpServerEnabled", value);
                ApplyMcpServer();
            }
        }
    }

    public int McpServerPort
    {
        get => _preferencesService.Get("McpServerPort", McpServerBase.PortDefault);

        set
        {
            if (McpServerPort != value)
            {
                _preferencesService.Set("McpServerPort", value);
                ApplyMcpServer();
            }
        }
    }

    public string McpServerAuthToken
    {
        get => _preferencesService.Get("McpServerAuthToken", McpServerBase.AuthTokenDefault);

        set
        {
            if (McpServerAuthToken != value)
            {
                _preferencesService.Set("McpServerAuthToken", value);
                ApplyMcpServer();
            }
        }
    }

    public void Dispose()
    {
        lock (_Lock)
        {
            _server?.Dispose();
            _server = null;

            _mcpServerBonjourPublisher.Stop();
        }
    }

    private void ApplyMcpServer()
    {
        lock (_Lock)
        {
            if (_server != null)
            {
                McpServerRemoved?.Invoke(this, _server);

                _server?.Dispose();
                _server = null;

                _mcpServerBonjourPublisher.Stop();
            }

            if (IsMcpServerEnabled)
            {
                if (_server == null)
                {
                    _server = new McpServer(McpServerPort, McpServerAuthToken);
                    Start();
                    McpServerAdded?.Invoke(this, _server);
                }
            }
        }
    }

    public void Start()
    {
        // start non-blocking
        Task.Run(() =>
        {
            _server?.StartAsync();
            _mcpServerBonjourPublisher.Publish(McpServerPort);
        });
    }
}
