namespace BrickController2.DeviceManagement;

/// <summary>
/// Interface definition for CaDA specific PlatformService
/// </summary>
public interface ICaDAPlatformService
{
    bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload);
}
