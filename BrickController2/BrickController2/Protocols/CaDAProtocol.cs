namespace BrickController2.Protocols;

public static class CaDAProtocol
{
    /// <summary>
    /// ManufacturerID for CaDA
    /// </summary>
    public const ushort ManufacturerID = 0xC200;

    /// <summary>
    /// CTXValue for Encryption
    /// </summary>
    public const byte CTXValue1 = 0x3f;

    /// <summary>
    /// CTXValue for Encryption
    /// </summary>
    public const byte CTXValue2 = 0x26;

    /// <summary>
    /// SeedArray
    /// </summary>
    public static readonly byte[] SeedArray =
    {
        67, // 0x43
        65, // 0x41
        82, // 0x52
    };

    /// <summary>
    /// HeaderArray
    /// </summary>
    public static readonly byte[] HeaderArray =
    {
        0x71,   // 0x71 (113)
        0x0f,   // 0x0f (15)
        0x55,   // 0x55 (85)
    };

    /// <summary>
    /// LookupTable
    /// </summary>
    private static readonly byte[] switchSheet = new byte[]
    {
        0xf4, 0xa8, 0xa0, 0x8c, 0x28, 0xec, 0x44, 0x00, 0x6c, 0x48, 0x24, 0x98, 0xd4, 0x9c, 0x0c, 0xac,
        0xa4, 0xbc, 0xcc, 0x80, 0x38, 0xe8, 0x5c, 0x1c, 0x94, 0xb0, 0xc8, 0x54, 0x34, 0x08, 0x74, 0xf0,
        0xdc, 0x14, 0xc4, 0xc0, 0x50, 0x18, 0x64, 0x7c, 0x70, 0x78, 0x88, 0x90, 0x58, 0x2c, 0xf8, 0x84,
        0x30, 0x68, 0x60, 0x04, 0x40, 0x4c, 0xe0, 0xb8, 0xd8, 0xfc, 0x20, 0x10, 0xe4, 0x3c, 0xd0, 0xb4,
    };

    public static void Encrypt(byte[] data)
    {
        byte bVar1;
        byte uVar2;

        if ((data[0] & 1) != 0)
        {
            bVar1 = data[2];
            data[2] = (byte)(data[2] & 0xf0);
            data[2] = (byte)(data[2] | data[6] & 0xf);
            data[6] = (byte)(data[6] & 0xf0);
            data[6] = (byte)(data[6] | bVar1 & 0xf);
        }
        if ((data[0] & 2) != 0)
        {
            bVar1 = data[2];
            data[2] = (byte)(data[2] & 0xf);
            data[2] = (byte)(data[2] | (byte)((data[5] & 0xf) << 4));
            data[5] = (byte)(data[5] & 0xf0);
            data[5] = (byte)(data[5] | (byte)((int)(uint)(bVar1 & 0xf0) >> 4));
        }
        if ((data[0] & 4) != 0)
        {
            uVar2 = data[3];
            data[3] = (byte)(data[3] & 0xf0);
            data[3] = (byte)(data[3] | (byte)((int)(data[4] & 0xf0) >> 4));
            data[4] = (byte)(data[4] & 0xf);
            data[4] = (byte)(data[4] | uVar2 << 4);
        }
        if ((data[0] & 8) != 0)
        {
            bVar1 = data[3];
            data[3] = (byte)(data[3] & 0xf);
            data[3] = (byte)(data[3] | (byte)((data[4] & 0xf) << 4));
            data[4] = (byte)(data[4] & 0xf0);
            data[4] = (byte)(data[4] | (byte)((int)(uint)(bVar1 & 0xf0) >> 4));
        }
        if ((data[0] & 0x10) != 0)
        {
            bVar1 = data[5];
            data[5] = (byte)(data[5] & 0xf);
            data[5] = (byte)(data[5] | data[7] & 0xf0);
            data[7] = (byte)(data[7] & 0xf);
            data[7] = (byte)(data[7] | bVar1 & 0xf0);
        }
        if ((data[0] & 0x20) != 0)
        {
            bVar1 = data[6];
            data[6] = (byte)(data[6] & 0xf);
            data[6] = (byte)(data[6] | (byte)((data[7] & 0xf) << 4));
            data[7] = (byte)(data[7] & 0xf0);
            data[7] = (byte)(data[7] | (byte)((int)(uint)(bVar1 & 0xf0) >> 4));
        }
        if ((data[0] & 0x40) != 0)
        {
            uVar2 = data[2];
            data[2] = (byte)(data[2] & 0xf0);
            data[2] = (byte)(data[2] | (byte)((int)(data[3] & 0xf0) >> 4));
            data[3] = (byte)(data[3] & 0xf);
            data[3] = (byte)(data[3] | uVar2 << 4);
        }
        if ((data[0] & 0x80) != 0)
        {
            bVar1 = data[2];
            data[2] = (byte)(data[2] & 0xf);
            data[2] = (byte)(data[2] | (byte)((data[3] & 0xf) << 4));
            data[3] = (byte)(data[3] & 0xf0);
            data[3] = (byte)(data[3] | (byte)((int)(uint)(bVar1 & 0xf0) >> 4));
        }
        data[2] = (byte)(data[2] ^ data[1] ^ 0x69);
        data[3] = (byte)(data[3] ^ data[1] ^ 0x69);
        data[4] = (byte)(data[4] ^ data[1] ^ 0x69);
        data[5] = (byte)(data[5] ^ data[1] ^ 0x69);
        data[6] = (byte)(data[6] ^ data[1] ^ 0x69);
        data[7] = (byte)(data[7] ^ data[1] ^ 0x69);
        for (int index = 0; index < 8; index++)
        {
            data[index] = (byte)(switchSheet[(int)(data[index] / 4)] + data[index] % 4);
        }
    }
}
