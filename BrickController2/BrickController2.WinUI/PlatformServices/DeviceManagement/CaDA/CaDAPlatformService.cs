using BrickController2.DeviceManagement.CaDA;
using BrickController2.Protocols;

namespace BrickController2.Windows.PlatformServices.DeviceManagement.CaDA;

public class CaDAPlatformService : ICaDAPlatformService
{
    private const int HeaderOffset = 15;
    private const int PayloadOffset = 3;
    private const int PayloadLength = 24 + PayloadOffset;

    public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload)
    {
        // JK:
        // Problem: Windows BT is limited - we can't advertise the same data as Android
        //
        // Idea: let's create data with the payload bytes at the same offset as Android
        //
        // On Android, there is first a flags section (-> 3 bytes), then the "vendor data" section (starting with length, type, company ID -> total 4 bytes).
        // -> The payload of the encrypted CaDA datagram starts at offset 7.
        //
        // The Windows BLE Advertiser device does not use the flags section, but only the "vendor data" section.
        // The header of the "vendor data" section starts with length, type, company ID -> total 4 bytes.
        //
        // Therefore, we need to place the encrypted CaDA payload at an offset of 3 bytes to have it at the same offset as Android.
        rfPayload = new byte[PayloadLength];
        int payloadLength = CryptTools.GetRfPayload(CaDAProtocol.SeedArray, CaDAProtocol.HeaderArray, rawData, HeaderOffset, CaDAProtocol.CTXValue1, CaDAProtocol.CTXValue2, rfPayload, PayloadOffset);

        return true;
    }
}
