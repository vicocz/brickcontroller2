using BrickController2.DeviceManagement.CaDA;
using BrickController2.Protocols;

namespace BrickController2.Droid.PlatformServices.DeviceManagement.CaDA;

public class CaDAPlatformService : ICaDAPlatformService
{
    private const int HeaderOffset = 15;
    private const int PayloadLength = 24;

    public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload)
    {
        rfPayload = new byte[PayloadLength];

        int payloadLength = CryptTools.GetRfPayload(CaDAProtocol.SeedArray, rawData, HeaderOffset, CaDAProtocol.CTXValue1, CaDAProtocol.CTXValue2, rfPayload);

        return true;
    }
}
