using BrickController2.DeviceManagement.JieStar;
using BrickController2.Protocols;

namespace BrickController2.Windows.PlatformServices.DeviceManagement.JieStar;

public class JieStarPlatformService : IJieStarPlatformService
{
    private const int HeaderOffset = 15;
    private const int PayloadOffset = 3;
    private const int PayloadLength = 24 + PayloadOffset;

    public bool TryGetRfPayload(byte ctxValue2, byte[] rawData, out byte[] rfPayload)
    {
        rfPayload = new byte[PayloadLength];
        int payloadLength = CryptTools.GetRfPayload(JieStarProtocol.SeedArray, JieStarProtocol.HeaderArray, rawData, HeaderOffset, JieStarProtocol.CTXValue1, ctxValue2, rfPayload, PayloadOffset);

        // fill rest of array
        for (int index = payloadLength + PayloadOffset; index < PayloadLength; index++)
        {
            rfPayload[index] = (byte)(index + 1);
        }

        return true;
    }
}
