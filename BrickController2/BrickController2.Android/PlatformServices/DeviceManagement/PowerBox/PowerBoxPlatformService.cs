using BrickController2.DeviceManagement.PowerBox;
using BrickController2.Protocols;

namespace BrickController2.Droid.PlatformServices.DeviceManagement.PowerBox;

public class PowerBoxPlatformService : IPowerBoxPlatformService
{
    private const int HeaderOffset = 15;
    private const int PayloadLength = 18;

    public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload)
    {
        rfPayload = new byte[PayloadLength];
        int payloadLength = CryptTools.GetRfPayload(PowerBoxProtocol.SeedArray, PowerBoxProtocol.HeaderArray, rawData, HeaderOffset, PowerBoxProtocol.CTXValue1, PowerBoxProtocol.CTXValue2, rfPayload);

        // fill rest of array
        for (int index = payloadLength; index < PayloadLength; index++)
        {
            rfPayload[index] = (byte)(index + 1);
        }

        return true;
    }
}
