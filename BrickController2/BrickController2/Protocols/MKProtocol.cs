using System;
using BrickController2.Helpers;

namespace BrickController2.Protocols;

/// <summary>
/// static class wich implements the encryption algorithm for the advertising data
/// </summary>
public static class MKProtocol
{
    /// <summary>
    /// ManufacturerID for MK
    /// </summary>
    public const ushort ManufacturerID = 0xFFF0;

    /// <summary>
    /// CTXValue for Encryption
    /// </summary>
    public const byte CTXValue1 = 0x3f;

    /// <summary>
    /// CTXValue for Encryption
    /// </summary>
    public const byte CTXValue2 = 0x25;

    /// <summary>
    /// Address array
    /// </summary>
    public static readonly byte[] SeedArray = { 0xC1, 0xC2, 0xC3, 0xC4, 0xC5 };

    /// <summary>
    /// crypt data-array with addr and ctxvalue
    /// </summary>
    /// <param name="seed">address array</param>
    /// <param name="data">data array to encrypt</param>
    /// <param name="headerOffset">offset for header data: android=0x0f (15) iOS=0xd (13)</param>
    /// <param name="ctxValue1">ctx value for encryption</param>
    /// <param name="ctxValue2">ctx value for encryption</param>
    /// <param name="rfPayload">crypted array</param>
    /// <returns>size of crypted array</returns>
    public static int GetRfPayload(byte[] seed, byte[] data, int headerOffset, byte ctxValue1, byte ctxValue2, byte[] rfPayload)
    {
        const int initValuesLength = 3;
        const int checksumLength = 2;

        int seedLength = seed.Length;
        int dataLength = data.Length;
        int resultArrayLength = initValuesLength + seedLength + dataLength + checksumLength;

        if (resultArrayLength > rfPayload.Length)
        {
            return 0;
        }

        //int headerOffset = 0x0f;                         // 0x0f (15)
        int seedOffset = headerOffset + initValuesLength;  // 0x12 (18) 
        int dataOffset = seedOffset + seedLength;
        int checksumOffset = dataOffset + dataLength;

        int resultBufferLength = checksumOffset + checksumLength;

        byte[] resultBuffer = new byte[resultBufferLength];

        // initValues (offset 15)
        resultBuffer[headerOffset + 0] = 0x71;   // 0x71 (113)
        resultBuffer[headerOffset + 1] = 0x0f;   // 0x0f (15)
        resultBuffer[headerOffset + 2] = 0x55;   // 0x55 (85)

        // reverse-copy seed-array into resultBuffer after initValues (offset 18)
        for (int index = 0; index < seedLength; index++)
        {
            resultBuffer[seedOffset + index] = seed[seedLength - 1 - index];
        }

        // invert bytes of initValues and seed-array in resultBuffer
        for (int index = 0; index < initValuesLength + seedLength; index++)
        {
            resultBuffer[headerOffset + index] = CryptTools.Invert8(resultBuffer[headerOffset + index]);
        }

        // copy dataArray into resultBuffer after initValues and seed-array
        Buffer.BlockCopy(data, 0, resultBuffer, dataOffset, dataLength);

        // calc checksum und copy to array
        ushort checksum = CryptTools.CheckCRC16(seed, data);
        resultBuffer.SetUInt16(checksum, checksumOffset);

        byte[] ctxArray1 = new byte[7];
        CryptTools.WhiteningInit(ctxValue1, ctxArray1); // 0x3f (63) -> ctx_0x3F = [1111111]
        CryptTools.WhiteningEncode(resultBuffer, seedOffset, seedLength + dataLength + checksumLength, ctxArray1);

        byte[] ctxArray2 = new byte[7];
        CryptTools.WhiteningInit(ctxValue2, ctxArray2); // ctxValue= 0x25 (37) -> ctx = [1101110]
        CryptTools.WhiteningEncode(resultBuffer, 0, resultBufferLength, ctxArray2);

        Buffer.BlockCopy(resultBuffer, headerOffset, rfPayload, 0, resultArrayLength);

        return resultArrayLength;
    }
}
