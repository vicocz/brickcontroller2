using System;
using BrickController2.Helpers;

namespace BrickController2.Protocols;

/// <summary>
/// static class containing functions needed by encryption algorithm for the advertising data
/// </summary>
public static class CryptTools
{
    /// <summary>
    /// crypt data-array with seed and ctxvalues
    /// </summary>
    /// <param name="seed">seed array</param>
    /// <param name="header">header array to encrypt</param>
    /// <param name="data">data array to encrypt</param>
    /// <param name="headerOffset">offset for header data: android=0x0f (15) iOS=0xd (13)</param>
    /// <param name="ctxValue1">ctx value1 for encryption</param>
    /// <param name="ctxValue2">ctx value2 for encryption</param>
    /// <param name="rfPayload">crypted array</param>
    /// <param name="rfPayloadOffset">start offset for crypted data</param>
    /// <returns>size of crypted array</returns>
    public static int GetRfPayload(byte[] seed, byte[] header, byte[] data, int headerOffset, byte ctxValue1, byte ctxValue2, byte[] rfPayload, int rfPayloadOffset = 0)
    {
        const int checksumLength = 2;
        int seedLength = seed.Length;
        int headerLength = header.Length;
        int dataLength = data.Length;

        int resultArrayLength = headerLength + seedLength + dataLength + checksumLength;
        if (resultArrayLength > rfPayload.Length - rfPayloadOffset)
        {
            return 0;
        }

        int seedOffset = headerOffset + headerLength;  // 0x12 (18) 
        int dataOffset = seedOffset + seedLength;
        int checksumOffset = dataOffset + dataLength;

        int resultBufferLength = checksumOffset + checksumLength;

        byte[] resultBuffer = new byte[resultBufferLength];

        Buffer.BlockCopy(header, 0, resultBuffer, headerOffset, header.Length);

        // reverse-copy seed-array into resultBuffer after initValues (offset 18)
        for (int index = 0; index < seedLength; index++)
        {
            resultBuffer[seedOffset + index] = seed[seedLength - 1 - index];
        }

        // invert bytes of initValues and seed-array in resultBuffer
        for (int index = 0; index < headerLength + seedLength; index++)
        {
            resultBuffer[headerOffset + index] = Reverse(resultBuffer[headerOffset + index]);
        }

        // copy dataArray into resultBuffer after initValues and seed-array
        Buffer.BlockCopy(data, 0, resultBuffer, dataOffset, dataLength);

        ushort checksum = CheckCRC16(seed, data);
        resultBuffer.SetUInt16(checksum, checksumOffset);

        byte[] ctxArray1 = new byte[7];
        WhiteningInit(ctxValue1, ctxArray1); // 0x3f (63): 1111111
        WhiteningEncode(resultBuffer, seedOffset, seedLength + dataLength + checksumLength, ctxArray1);

        byte[] ctxArray2 = new byte[7];
        WhiteningInit(ctxValue2, ctxArray2); // 0x26 (38): 1101110
        WhiteningEncode(resultBuffer, 0, resultBufferLength, ctxArray2);

        Buffer.BlockCopy(resultBuffer, headerOffset, rfPayload, rfPayloadOffset, resultArrayLength);

        return resultArrayLength;
    }

    /// <summary>
    /// Reverses the bit order of an 8-bit unsigned integer.
    /// </summary>
    /// <remarks>This method takes an 8-bit unsigned integer and reverses the order of its bits. For example,
    /// if the input is <c>0b00000001</c>, the output will be <c>0b10000000</c>.</remarks>
    /// <param name="value">The 8-bit unsigned integer whose bits are to be reversed.</param>
    /// <returns>An 8-bit unsigned integer with the bits of <paramref name="value"/> reversed.</returns>
    public static byte Reverse(byte value)
    {
        /* (bitwise swap version)
        // Swap odd and even bits
        value = (byte)(((value & 0xAA) >> 1) | ((value & 0x55) << 1));
        // Swap consecutive pairs
        value = (byte)(((value & 0xCC) >> 2) | ((value & 0x33) << 2));
        // Swap nibbles
        value = (byte)(((value & 0xF0) >> 4) | ((value & 0x0F) << 4));
        */

        // (bit-twiddling hack version)
        value = (byte)(((value * 0x0802U & 0x22110U) | (value * 0x8020U & 0x88440U)) * 0x10101U >> 16);
        return value;
    }

    /// <summary>
    /// Reverses the bit order of a 16-bit unsigned integer.
    /// </summary>
    /// <remarks>This method swaps the positions of the bits in the input value, effectively reversing their
    /// order. For example, if the input value is represented in binary as <c>0000000000001011</c>, the output will be
    /// <c>1101000000000000</c>.</remarks>
    /// <param name="value">The 16-bit unsigned integer whose bits are to be reversed.</param>
    /// <returns>A 16-bit unsigned integer with the bit order of <paramref name="value"/> reversed.</returns>
    public static ushort Reverse(ushort value)
    {
        // Swap odd and even bits
        value = (ushort)(((value & 0xAAAA) >> 1) | ((value & 0x5555) << 1));
        // Swap consecutive pairs
        value = (ushort)(((value & 0xCCCC) >> 2) | ((value & 0x3333) << 2));
        // Swap nibbles
        value = (ushort)(((value & 0xF0F0) >> 4) | ((value & 0x0F0F) << 4));
        // Swap bytes
        value = (ushort)((value >> 8) | (value << 8));
        return value;
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
            byte cVar1 = Reverse(array2[index]);

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
        ushort result_inverse = Reverse((ushort)result);
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
