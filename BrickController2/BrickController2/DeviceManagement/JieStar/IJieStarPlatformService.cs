namespace BrickController2.DeviceManagement.JieStar;

/// <summary>
/// Interface definition for JieStar specific PlatformService
/// </summary>
public interface IJieStarPlatformService
{
    bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload);
}
