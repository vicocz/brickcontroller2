using BrickController2.DeviceManagement.PowerBox;
using BrickController2.Protocols;

namespace BrickController2.Windows.PlatformServices.DeviceManagement.PowerBox;

public class PowerBoxPlatformService : IPowerBoxPlatformService
{
    private const int HeaderOffset = 15;
    private const int PayloadOffset = 3;
    private const int PayloadLength = 23 + PayloadOffset;

    public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload)
    {
        rfPayload = new byte[PayloadLength];
        int payloadLength = CryptTools.GetRfPayload(PowerBoxProtocol.SeedArray, PowerBoxProtocol.HeaderArray, rawData, HeaderOffset, PowerBoxProtocol.CTXValue1, PowerBoxProtocol.CTXValue2, rfPayload, PayloadOffset);

        // fill rest of array
        for (int index = payloadLength + PayloadOffset; index < PayloadLength; index++)
        {
            rfPayload[index] = (byte)(index + 1);
        }

        return true;
    }
}
