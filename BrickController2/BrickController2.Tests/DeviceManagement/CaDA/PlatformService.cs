using BrickController2.DeviceManagement.CaDA;
using System;
using System.Collections.Generic;

namespace BrickController2.Tests.DeviceManagement.CaDA;

public static class PlatformService
{
    public class Default : ICaDAPlatformService
    {
        public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload)
        {
            rfPayload = [.. rawData];
            return true;
        }
    }

    public class IOS : ICaDAPlatformService
    {
        public static readonly byte[] SessionId = [(byte)Random.Shared.Next(), 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, (byte)Random.Shared.Next()];
        public static readonly byte[] Prefix = [0xC0, 0x00];

        public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload) => throw new NotImplementedException();

        public bool TryGetRfPayloadV2(ReadOnlySpan<byte> rawData, out byte[] rfPayload)
        {
            rfPayload = new byte[Prefix.Length + rawData.Length + SessionId.Length];

            // prefix
            Prefix.CopyTo(rfPayload.AsSpan());
            rawData.CopyTo(rfPayload.AsSpan(Prefix.Length));
            SessionId.CopyTo(rfPayload.AsSpan(Prefix.Length + rawData.Length));

            return rawData.Length == 16;
        }
    }
}

