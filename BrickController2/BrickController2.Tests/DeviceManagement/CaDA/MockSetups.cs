using BrickController2.DeviceManagement.CaDA;
using BrickController2.Protocols;
using Moq;

namespace BrickController2.Tests.DeviceManagement.CaDA;

public static class MockSetups
{
    public static Mock<ICaDAPlatformService> TryGetRfPayload_ForIosPlatform(this Mock<ICaDAPlatformService> mock)
    {
        const int HeaderOffset = 13;
        const int PayloadLength = 26;

        mock.Setup(m => m.TryGetRfPayload(It.IsAny<byte[]>(), out It.Ref<byte[]>.IsAny))
            .Callback((byte[] rawData, out byte[] rfPayload) =>
            {
                rfPayload = new byte[PayloadLength];
                int payloadLength = CryptTools.GetRfPayload(CaDAProtocol.SeedArray,
                    CaDAProtocol.HeaderArray,
                    rawData,
                    HeaderOffset,
                    CaDAProtocol.CTXValue1,
                    CaDAProtocol.CTXValue2,
                    rfPayload);

                // fill rest of array
                byte bVar = 0x18; // initial value
                for (int index = payloadLength; index < PayloadLength; index++)
                {
                    rfPayload[index] = bVar++;
                }
            })
            .Returns(true);

        return mock;
    }
}
