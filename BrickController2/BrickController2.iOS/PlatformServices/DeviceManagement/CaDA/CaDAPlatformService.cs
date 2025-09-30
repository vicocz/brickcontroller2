using BrickController2.DeviceManagement.CaDA;
using BrickController2.Protocols;

namespace BrickController2.iOS.PlatformServices.DeviceManagement.CaDA;

public class CaDAPlatformService : ICaDAPlatformService
{
    private const int HeaderOffset = 13;
    private const int PayloadLength = 26;

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
}
