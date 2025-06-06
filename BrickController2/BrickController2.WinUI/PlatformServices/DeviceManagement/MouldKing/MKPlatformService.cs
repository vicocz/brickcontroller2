using BrickController2.DeviceManagement.MouldKing;
using BrickController2.Protocols;

namespace BrickController2.Windows.PlatformServices.DeviceManagement.MouldKing;

public class MKPlatformService : IMKPlatformService
{
    private const int HeaderOffset = 15;
    private const int PayloadLength = 26;

    public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload)
    {
        rfPayload = new byte[PayloadLength];
        int payloadLength = CryptTools.GetRfPayload(MKProtocol.SeedArray, MKProtocol.HeaderArray, rawData, HeaderOffset, MKProtocol.CTXValue1, MKProtocol.CTXValue2, rfPayload);

        // fill rest of array
        byte bVar = 0x12; // initial value
        for (int index = payloadLength; index < PayloadLength; index++)
        {
            rfPayload[index] = bVar++;
        }

        return true;
    }
}
