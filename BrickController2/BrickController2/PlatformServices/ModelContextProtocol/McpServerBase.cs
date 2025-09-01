namespace BrickController2.PlatformServices.ModelContextProtocol;

public abstract class McpServerBase
{
    public const int PortMin = 1;
    public const int PortMax = 65535;
    public const int PortDefault = 5000;

    public const string AuthTokenDefault = "";
}
