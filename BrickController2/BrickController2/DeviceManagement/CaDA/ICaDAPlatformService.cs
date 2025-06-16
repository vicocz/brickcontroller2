namespace BrickController2.DeviceManagement.CaDA;

/// <summary>
/// Interface definition for CaDA specific PlatformService
/// </summary>
public interface ICaDAPlatformService
{
    bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload);
}
