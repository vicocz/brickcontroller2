namespace BrickController2.PlatformServices.ModelContextProtocol;

/// <summary>
/// Interface for managing the MCP (Model Context Protocol) server service.
/// </summary>
public interface IMcpServerService
{
    /// <summary>
    /// Gets a value indicating whether the MCP server is currently available.
    /// </summary>
    bool IsMcpServerAvailable { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the MCP server is enabled.
    /// </summary>
    bool IsMcpServerEnabled { get; set; }

    /// <summary>
    /// Gets or sets the port number for the MCP server.
    /// </summary>
    int McpServerPort { get; set; }

    /// <summary>
    /// Gets or sets the authentication token for the MCP server.
    /// </summary>
    string McpServerAuthToken { get; set; }
}
