namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// Interface definition for MouldKing specific PlatformService
/// </summary>
public interface IMKPlatformService
{
    bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload);
}
