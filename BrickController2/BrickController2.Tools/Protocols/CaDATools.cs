using BrickController2.Protocols;

namespace BrickController2.Tools.Protocols;

public static class CaDATools
{
    // Reverse lookup table for SwitchSheet transformation
    private static readonly byte[] ReverseSwitchSheet = CreateReverseSwitchSheet();

    private static byte[] CreateReverseSwitchSheet()
    {
        var table = new byte[256];
        for (int orig = 0; orig < 256; orig++)
        {
            byte candidate = (byte)(CaDAProtocol.SwitchSheet[orig / 4] + orig % 4);
            table[candidate] = (byte)orig;
        }
        return table;
    }

    public static void Decrypt(byte[] data)
    {
        // Inverse of the last for-loop in Encrypt
        for (int index = 0; index < 8; index++)
        {
            byte val = data[index];

            // Use reverse lookup table to find the original value
            data[index] = ReverseSwitchSheet[val];
        }

        // Inverse of the XOR and 0x69 step
        data[2] = (byte)(data[2] ^ data[1] ^ 0x69);
        data[3] = (byte)(data[3] ^ data[1] ^ 0x69);
        data[4] = (byte)(data[4] ^ data[1] ^ 0x69);
        data[5] = (byte)(data[5] ^ data[1] ^ 0x69);
        data[6] = (byte)(data[6] ^ data[1] ^ 0x69);
        data[7] = (byte)(data[7] ^ data[1] ^ 0x69);

        // Inverse of the bit manipulations, in reverse order
        if ((data[0] & 0x80) != 0)
        {
            byte saved = data[3];
            data[3] = (byte)((data[3] & 0xf0) | ((data[2] & 0xf0) >> 4));
            data[2] = (byte)((data[2] & 0xf) | ((saved & 0xf) << 4));
        }
        if ((data[0] & 0x40) != 0)
        {
            byte saved = data[3];
            data[3] = (byte)((data[3] & 0xf) | ((data[2] & 0xf) << 4));
            data[2] = (byte)((data[2] & 0xf0) | ((saved & 0xf0) >> 4));
        }
        if ((data[0] & 0x20) != 0)
        {
            byte saved = data[7];
            data[7] = (byte)((data[7] & 0xf0) | ((data[6] & 0xf0) >> 4));
            data[6] = (byte)((data[6] & 0xf) | ((saved & 0xf) << 4));
        }
        if ((data[0] & 0x10) != 0)
        {
            byte saved = data[7];
            data[7] = (byte)((data[7] & 0xf) | (data[5] & 0xf0));
            data[5] = (byte)((data[5] & 0xf) | (saved & 0xf0));
        }
        if ((data[0] & 0x08) != 0)
        {
            byte saved = data[4];
            data[4] = (byte)((data[4] & 0xf0) | ((data[3] & 0xf0) >> 4));
            data[3] = (byte)((data[3] & 0xf) | ((saved & 0xf) << 4));
        }
        if ((data[0] & 0x04) != 0)
        {
            byte saved = data[4];
            data[4] = (byte)((data[4] & 0xf) | (data[3] << 4));
            data[3] = (byte)((data[3] & 0xf0) | ((saved & 0xf0) >> 4));
        }
        if ((data[0] & 0x02) != 0)
        {
            byte saved = data[5];
            data[5] = (byte)((data[5] & 0xf0) | ((data[2] & 0xf0) >> 4));
            data[2] = (byte)((data[2] & 0xf) | ((saved & 0xf) << 4));
        }
        if ((data[0] & 0x01) != 0)
        {
            byte saved = data[6];
            data[6] = (byte)((data[6] & 0xf0) | (data[2] & 0xf));
            data[2] = (byte)((data[2] & 0xf0) | (saved & 0xf));
        }
    }
}
