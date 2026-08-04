namespace BrickController2.DeviceManagement.PowerBox;

/// <summary>
/// Interface definition for PowerBox specific PlatformService
/// </summary>
public interface IPowerBoxPlatformService
{
    bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload);
}
