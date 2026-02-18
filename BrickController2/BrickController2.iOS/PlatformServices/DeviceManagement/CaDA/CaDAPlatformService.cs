using BrickController2.DeviceManagement.CaDA;
using BrickController2.Protocols;
using System;

namespace BrickController2.iOS.PlatformServices.DeviceManagement.CaDA;

public class CaDAPlatformService : ICaDAPlatformService
{
    private const int HeaderOffset = 13;
    private const int PayloadLength = 26;

    private const int SessionLength = 8;
    private const int PayloadRev2Length = 16;

    // Session - 8 bytes per application run
    private static readonly Lazy<byte[]> _sessionPostfix = new(() =>
    {
        var session = new byte[SessionLength];
        Random.Shared.NextBytes(session);
        return session;
    });

    public ReadOnlySpan<byte> Session => _sessionPostfix.Value;

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

    public bool TryGetRfPayload(ushort manufacturerId, ReadOnlySpan<byte> rawData, out byte[] rfPayload)
    {
        rfPayload = new byte[2 + rawData.Length + PayloadLength];

        BitConverter.TryWriteBytes(rfPayload, manufacturerId);

        rawData.CopyTo(rfPayload.AsSpan(2));
        Session.CopyTo(rfPayload.AsSpan(2 + rawData.Length));

        return rawData.Length == PayloadRev2Length;
    }
}
