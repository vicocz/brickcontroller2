using BrickController2.DeviceManagement.JieStar;
using BrickController2.Protocols;

namespace BrickController2.iOS.PlatformServices.DeviceManagement.JieStar;

public class JieStarPlatformService : IJieStarPlatformService
{
    private const int HeaderOffset = 13;
    private const int PayloadLength = 26;

    public bool TryGetRfPayload(byte ctxValue2, byte[] rawData, out byte[] rfPayload)
    {
        rfPayload = new byte[PayloadLength];
        int payloadLength = CryptTools.GetRfPayload(JieStarProtocol.SeedArray, JieStarProtocol.HeaderArray, rawData, HeaderOffset, JieStarProtocol.CTXValue1, ctxValue2, rfPayload);

        // fill rest of array
        byte bVar = 0x12; // initial value
        for (int index = payloadLength; index < PayloadLength; index++)
        {
            rfPayload[index] = bVar++;
        }

        return true;
    }
}
