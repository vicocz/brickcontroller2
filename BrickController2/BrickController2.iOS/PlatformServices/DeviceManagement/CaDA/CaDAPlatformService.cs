using BrickController2.DeviceManagement.CaDA;
using BrickController2.Protocols;
using System;

namespace BrickController2.iOS.PlatformServices.DeviceManagement.CaDA;

public class CaDAPlatformService : ICaDAPlatformService
{
    private const int HeaderOffset = 13;
    private const int PayloadLength = 26;
    private const int V2SessionLength = 8; // Rev2 session, IOS only
    private const int V2PayloadLength = 16;

    private static readonly ReadOnlyMemory<byte> _prefix = new([0xC0, 0x00]);
    private readonly ReadOnlyMemory<byte> _session;

    public CaDAPlatformService()
    {
        // generate random session postfix
        var session = new byte[V2SessionLength];
        Random.Shared.NextBytes(session);
        _session = session;
    }

    public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload)
    {
        rfPayload = new byte[PayloadLength];
        int payloadLength = CryptTools.GetRfPayload(CaDAProtocol.SeedArray, CaDAProtocol.HeaderArray, rawData, HeaderOffset, CaDAProtocol.CTXValue1, CaDAProtocol.CTXValue2, rfPayload);

        // fill rest of array
        byte bVar = 0x18; // initial value
        for (int index = payloadLength; index < PayloadLength; index++)
        {
            rfPayload[index] = bVar++;
        }

        return true;
    }

    public bool TryGetRfPayloadV2(ReadOnlySpan<byte> rawData, out byte[] rfPayload)
    {
        rfPayload = new byte[_prefix.Length + rawData.Length + V2SessionLength];

        _prefix.CopyTo(rfPayload);
        rawData.CopyTo(rfPayload.AsSpan(_prefix.Length));
        _session.CopyTo(rfPayload.AsMemory(_prefix.Length + rawData.Length));

        return rawData.Length == V2PayloadLength;
    }
}
