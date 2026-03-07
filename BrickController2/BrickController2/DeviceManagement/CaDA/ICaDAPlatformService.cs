using System;

namespace BrickController2.DeviceManagement.CaDA;

/// <summary>
/// Interface definition for CaDA specific PlatformService
/// </summary>
public interface ICaDAPlatformService
{
    public const int DefaultPayloadRev2Length = 16;

    bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload);

    bool TryGetRfPayloadRev2(ReadOnlySpan<byte> rawData, out byte[] rfPayload)
    {
        // default implementation: copy raw data
        rfPayload = rawData.ToArray();
        return rawData.Length == DefaultPayloadRev2Length;
    }
}
