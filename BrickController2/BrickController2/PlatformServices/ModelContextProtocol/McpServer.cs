using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.PlatformServices.ModelContextProtocol;
public sealed class McpServer : McpServerBase, IDisposable
{
    private const string ToolSetChannels = "set_channels";
    private const string ToolStop = "stop";

    private readonly string _url;
    private readonly bool _enableCors;
    private readonly object _channelLock = new();
    private readonly List<ChannelValue> _channelStates = new();
    private readonly string _authToken;

    private WebServer? _server;

    public event Action<List<ChannelValue>>? ChannelStatesUpdated;

    public McpServer(int port, string authToken = "")
       : this($"http://*:{port}", true, authToken)
    {
    }

    public McpServer(string url = "http://*:5000", bool enableCors = true, string authToken = "")
    {
        _url = url;
        _enableCors = enableCors;
        _authToken = authToken;
    }

    public void Dispose()
    {
        if (_server != null)
        {
            _server.Dispose();
            _server = null;
        }
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        if (_server != null)
        {
            return Task.CompletedTask;
        }

        var server = new WebServer(o => o
            .WithUrlPrefix(_url)
            .WithMode(HttpListenerMode.EmbedIO))
            .WithLocalSessionManager();

        // CORS (if available in your EmbedIO version)
        if (_enableCors)
        {
            try
            {
                server.WithCors(origins: "*", headers: "*", methods: "*");
            }
            catch
            {
                // Optional: add a manual CORS ActionModule fallback if needed.
            }
        }

        server.WithWebApi("/", m => m
            .WithController(() => new McpController(this)));

        _server = server;

        // Start is non-blocking
        return _server.RunAsync();
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        if (_server == null)
        {
            return Task.CompletedTask;
        }

        _server.Dispose();
        _server = null;

        return Task.CompletedTask;
    }

    // --- State & update helpers ---

    private List<ChannelValue> Snapshot()
    {
        lock (_channelLock) { return _channelStates.ToList(); }
    }

    private List<ChannelValue> SetChannels(IDictionary<int, double> map)
    {
        List<ChannelValue> channelStates;

        lock (_channelLock)
        {
            _channelStates.Clear();
            _channelStates.AddRange(map.Select(kv => new ChannelValue { Channel = kv.Key, Value = kv.Value }));
            channelStates = _channelStates.ToList();
        }

        ChannelStatesUpdated?.Invoke(channelStates);

        return channelStates;
    }

    private List<ChannelValue> StopAll()
    {
        List<ChannelValue> channelStates;

        lock (_channelLock)
        {
            for (int i = 0; i < _channelStates.Count; i++)
            {
                _channelStates[i].Value = 0.0;
            }
            channelStates = _channelStates.ToList();
        }

        ChannelStatesUpdated?.Invoke(channelStates);

        return channelStates;
    }


    // --- Auth helper ---
    private bool IsAuthorized(WebApiController controller)
    {
        // no token, no check
        if (string.IsNullOrEmpty(_authToken))
        {
            return true;
        }

        var authHeader = controller.Request.Headers["Authorization"];

        if (string.IsNullOrEmpty(authHeader))
        {
            return false;
        }

        var token = authHeader.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        return string.Equals(token, _authToken, StringComparison.Ordinal);
    }

    // --- Contracts ---

    public record ChannelValue
    {
        [JsonPropertyName("channel")]
        public int Channel { get; set; }

        [JsonPropertyName("value")]
        public double Value { get; set; }
    }

    public record ExecuteRequest
    {
        public string name { get; init; } = "";
        public Dictionary<string, object?>? arguments { get; init; }
    }

    public record ToolDescription
    {
        public string name { get; init; } = "";
        public string? description { get; init; }
        public JsonSchema? parameters { get; init; }
    }

    public record JsonSchema
    {
        public string type { get; init; } = "object";
        public Dictionary<string, object?>? properties { get; init; }
        public string[]? required { get; init; }
        public bool additionalProperties { get; init; } = false;
    }

    // --- Web API Controller ---

    private sealed class McpController : WebApiController
    {
        private readonly McpServer _svc;

        public McpController(McpServer svc) => _svc = svc;

        [Route(HttpVerbs.Get, "/health")]
        public object Health()
        {
            if (!_svc.IsAuthorized(this))
            {
                return HttpStatusCode.Unauthorized;
            }

            return new
            {
                ok = true,
                service = "mcp-brickcontroller",
                time = DateTimeOffset.UtcNow
            };
        }

        [Route(HttpVerbs.Get, "/mcp/channels")]
        public object GetChannels()
        {
            if (!_svc.IsAuthorized(this))
            {
                return HttpStatusCode.Unauthorized;
            }

            return new
            {
                channels = _svc.Snapshot()
            };
        }

        [Route(HttpVerbs.Get, "/mcp/capabilities")]
        public object Capabilities()
        {
            if (!_svc.IsAuthorized(this))
            {
                return HttpStatusCode.Unauthorized;
            }

            var setChannelsTool = new ToolDescription
            {
                name = ToolSetChannels,
                description = "Sets one or more channel values, each as a channel number and a value between -1.0 and 1.0.",
                parameters = new JsonSchema
                {
                    type = "object",
                    properties = new Dictionary<string, object?>
                    {
                        ["channels"] = new Dictionary<string, object?>
                        {
                            ["type"] = "array",
                            ["items"] = new Dictionary<string, object?>
                            {
                                ["type"] = "object",
                                ["properties"] = new Dictionary<string, object?>
                                {
                                    ["channel"] = new Dictionary<string, object?>
                                    {
                                        ["type"] = "integer",
                                        ["description"] = "Channel number (integer)."
                                    },
                                    ["value"] = new Dictionary<string, object?>
                                    {
                                        ["type"] = "number",
                                        ["minimum"] = -1.0,
                                        ["maximum"] = 1.0,
                                        ["description"] = "Channel value between -1.0 and 1.0."
                                    }
                                },
                                ["required"] = new[] { "channel", "value" }
                            },
                            ["description"] = "Array of channel/value objects."
                        }
                    },
                    required = new[] { "channels" },
                    additionalProperties = false
                }
            };

            var stopTool = new ToolDescription
            {
                name = ToolStop,
                description = "Stops all activity by setting all channels to 0.",
                parameters = new JsonSchema
                {
                    type = "object",
                    properties = new Dictionary<string, object?>(),
                    required = Array.Empty<string>(),
                    additionalProperties = false
                }
            };

            return new
            {
                capabilities = new[] { "tools" },
                tools = new[] { setChannelsTool, stopTool },
                server = new { name = "android-mcp-brickcontroller", version = "0.1.0" }
            };
        }

        [Route(HttpVerbs.Post, "/mcp/tools/execute")]
        public async Task<object> Execute()
        {
            if (!_svc.IsAuthorized(this))
            {
                return HttpStatusCode.Unauthorized;
            }

            var req = await HttpContext.GetRequestDataAsync<McpServer.ExecuteRequest>();

            if (string.Equals(req.name, ToolSetChannels, StringComparison.OrdinalIgnoreCase))
            {
                if (req.arguments is null || !req.arguments.TryGetValue("channels", out var channelsObj))
                    return new { error = "invalid_arguments", message = "channels (array) is required." };

                var map = new Dictionary<int, double>();

                if (channelsObj is JsonElement je && je.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in je.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Object)
                            return new { error = "invalid_arguments", message = "Each channel entry must be an object." };

                        if (!item.TryGetProperty("channel", out var chanProp) || !chanProp.TryGetInt32(out var channel))
                            return new { error = "invalid_arguments", message = "channel (int) is required." };

                        if (!item.TryGetProperty("value", out var valProp) || !valProp.TryGetDouble(out var value))
                            return new { error = "invalid_arguments", message = "value (float) is required." };

                        if (value < -1.0 || value > 1.0)
                            return new { error = "invalid_arguments", message = "value must be between -1.0 and 1.0." };

                        map[channel] = value; // last write wins
                    }
                }
                else if (channelsObj is IEnumerable<object?> arrObj)
                {
                    foreach (var o in arrObj)
                    {
                        if (o is not Dictionary<string, object?> dict ||
                            !dict.TryGetValue("channel", out var chO) ||
                            !dict.TryGetValue("value", out var valO))
                            return new { error = "invalid_arguments", message = "Invalid channel entry." };

                        var channel = Convert.ToInt32(chO);
                        var value = Convert.ToDouble(valO);
                        if (value < -1.0 || value > 1.0)
                            return new { error = "invalid_arguments", message = "value must be between -1.0 and 1.0." };

                        map[channel] = value;
                    }
                }
                else
                {
                    return new { error = "invalid_arguments", message = "channels must be an array." };
                }

                var snapshot = _svc.SetChannels(map);
                return new
                {
                    ok = true,
                    tool = ToolSetChannels,
                    output = new
                    {
                        message = $"Channels updated: {string.Join(", ", snapshot.Select(c => $"ch{c.Channel}={c.Value:0.##}"))}",
                        channels = snapshot
                    }
                };
            }
            else if (string.Equals(req.name, ToolStop, StringComparison.OrdinalIgnoreCase))
            {
                var snapshot = _svc.StopAll();
                return new
                {
                    ok = true,
                    tool = ToolStop,
                    output = new
                    {
                        message = "All channels set to 0 (stopped).",
                        channels = snapshot
                    }
                };
            }

            return new
            {
                error = "unknown_tool",
                message = $"Tool '{req.name}' is not supported.",
                supported = new[] { ToolSetChannels, ToolStop }
            };
        }
    }
}
