using BrickController2.DeviceManagement;
using BrickController2.Protocols;

namespace BrickController2.Droid.PlatformServices.DeviceManagement;

public class MKPlatformService : IMKPlatformService
{
    private const int HeaderOffset = 15;
    private const int PayloadLength = 24;

    public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload)
    {
        rfPayload = new byte[PayloadLength];
        int payloadLength = MKProtocol.GetRfPayload(MKProtocol.SeedArray, rawData, HeaderOffset, MKProtocol.CTXValue1, MKProtocol.CTXValue2, rfPayload);

        // fill rest of array
        for (int index = payloadLength; index < PayloadLength; index++)
        {
            rfPayload[index] = (byte)(index + 1);
        }

        return true;
    }
}
