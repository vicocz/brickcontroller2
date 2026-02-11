using BrickController2.DeviceManagement.CaDA;
using BrickController2.Protocols;
using System;

namespace BrickController2.Droid.PlatformServices.DeviceManagement.CaDA;

public class CaDAPlatformService : ICaDAPlatformService
{
    private const int HeaderOffset = 15;
    private const int PayloadLength = 24;

    private const int PayloadRev2Length = 16;

    public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload)
    {
        rfPayload = new byte[PayloadLength];

        int payloadLength = CryptTools.GetRfPayload(CaDAProtocol.SeedArray, CaDAProtocol.HeaderArray, rawData, HeaderOffset, CaDAProtocol.CTXValue1, CaDAProtocol.CTXValue2, rfPayload);

        return true;
    }

    public bool TryGetRfPayload(ushort manufacturerId, ReadOnlySpan<byte> rawData, out byte[] rfPayload)
    {
        // copy past data
        rfPayload = rawData.ToArray();
        return rawData.Length == PayloadRev2Length;
    }
}
