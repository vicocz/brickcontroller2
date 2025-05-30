namespace BrickController2.Protocols;

/// <summary>
/// static class containing functions needed by encryption algorithm for the advertising data
/// </summary>
public static class CryptTools
{
    /// <summary>
    /// inverts the bits of a given byte
    /// </summary>
    /// <param name="value">byte to invert</param>
    /// <returns>inverted byte</returns>
    public static byte Invert8(byte value)
    {
        int result = 0;
        for (byte index = 0; index < 8; index++)
        {
            if ((value & 1 << (index & 0x1f)) != 0)
            {
                result |= (byte)(1 << (7 - index & 0x1f));
            }
        }
        return (byte)result;
    }

    /// <summary>
    /// inverts the bits of a given short
    /// </summary>
    /// <param name="value">short to invert</param>
    /// <returns>inverted short</returns>
    public static ushort Invert16(ushort value)
    {
        int result = 0;
        for (byte index = 0; index < 0x10; index++)
        {
            if (((uint)value & 1 << (index & 0x1f)) != 0)
            {
                result |= (ushort)(1 << (0xf - index & 0x1f));
            }
        }
        return (ushort)result;
    }

    /// <summary>
    /// calculate crc16
    /// </summary>
    /// <param name="array1">first array</param>
    /// <param name="array2">second array</param>
    /// <returns></returns>
    public static ushort CheckCRC16(byte[] array1, byte[] array2)
    {
        int array1Length = array1.Length;

        int result = 0xffff;
        for (int index = 0; index < array1Length; index++)
        {
            result ^= (ushort)(array1[array1Length -1 - index] << 8);

            for (int local_24 = 0; local_24 < 8; local_24++)
            {
                if ((result & 0x8000) == 0)
                {
                    result = result << 1;
                }
                else
                {
                    result = result << 1 ^ 0x1021;
                }
            }
        }

        int array2Length = array2.Length;
        for (int index = 0; index < array2Length; index++)
        {
            byte cVar1 = Invert8(array2[index]);

            result = result ^ (ushort)(cVar1 << 8);

            for (int local_2c = 0; local_2c < 8; local_2c++)
            {
                if ((result & 0x8000) == 0)
                {
                    result = result << 1;
                }
                else
                {
                    result = result << 1 ^ 0x1021;
                }
            }
        }
        ushort result_inverse = Invert16((ushort)result);
        return (ushort)(result_inverse ^ 0xffff);
    }

    /// <summary>
    /// initialize ctx array
    /// </summary>
    /// <param name="val">value to init</param>
    /// <param name="ctx">byte[7] to be initialized</param>
    public static void WhiteningInit(byte val, byte[] ctx)
    {
        ctx[0] = 1;
        ctx[1] = (byte)(val >> 5 & 1);
        ctx[2] = (byte)(val >> 4 & 1);
        ctx[3] = (byte)(val >> 3 & 1);
        ctx[4] = (byte)(val >> 2 & 1);
        ctx[5] = (byte)(val >> 1 & 1);
        ctx[6] = (byte)(val & 1);
    }

    /// <summary>
    /// encode byte[]
    /// </summary>
    /// <param name="data">byte[]</param>
    /// <param name="dataStartIndex">startindex of bytes to encode</param>
    /// <param name="len">length of bytearray</param>
    /// <param name="ctx">ctx array</param>
    public static void WhiteningEncode(byte[] data, int dataStartIndex, int len, byte[] ctx)
    {
        for (int index = 0; index < len; index++)
        {
            byte currentByte = data[dataStartIndex + index];
            int currentResult = 0;
            for (byte bitIndex = 0; bitIndex < 8; bitIndex++)
            {
                byte uVar2 = WhiteningOutput(ctx);
                currentResult = (int)((uVar2 ^ currentByte >> (bitIndex & 0x1f) & 1U) << (bitIndex & 0x1f)) + currentResult;
            }
            data[dataStartIndex + index] = (byte)currentResult;
        }
        return;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    private static byte WhiteningOutput(byte[] ctx)
    {
        byte value_3 = ctx[3];
        byte value_6 = ctx[6];
        ctx[3] = ctx[2];
        ctx[2] = ctx[1];
        ctx[1] = ctx[0];
        ctx[0] = ctx[6];
        ctx[6] = ctx[5];
        ctx[5] = ctx[4];
        ctx[4] = (byte)(value_3 ^ value_6);
        return ctx[0];
    }
}
