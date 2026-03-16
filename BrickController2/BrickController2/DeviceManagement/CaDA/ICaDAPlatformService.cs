using System;

namespace BrickController2.DeviceManagement.CaDA;

/// <summary>
/// Interface definition for CaDA specific PlatformService
/// </summary>
public interface ICaDAPlatformService
{
    private const int V2PayloadLength = 16;
    bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload);

    bool TryGetRfPayloadV2(ReadOnlySpan<byte> rawData, out byte[] rfPayload)
    {
        // default implementation: copy raw data
        rfPayload = rawData.ToArray();
        return rawData.Length == V2PayloadLength;
    }
}
