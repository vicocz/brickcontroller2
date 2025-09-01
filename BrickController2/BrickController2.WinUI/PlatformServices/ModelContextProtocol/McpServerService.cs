using BrickController2.PlatformServices.ModelContextProtocol;
using BrickController2.UI.Services.Preferences;
using System;
using System.Threading.Tasks;

namespace BrickController2.Windows.PlatformServices.ModelContextProtocol;

public class McpServerService : IMcpServerService, IDisposable
{
    private readonly object _Lock = new object();
    private readonly IPreferencesService _preferencesService;
    private McpServer? _server;

    public McpServerService(IPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
        _server = null;

        ApplyMcpServer();
    }

    public event EventHandler<McpServer>? McpServerAdded;
    public event EventHandler<McpServer>? McpServerRemoved;

    public bool IsMcpServerAvailable => true;

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

    public McpServer? Server => _server;

    public void Dispose()
    {
        lock (_Lock)
        {
            _server?.Dispose();
            _server = null;
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
        Task.Run(() => _server?.StartAsync());
    }
}
