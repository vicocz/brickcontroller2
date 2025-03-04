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
    public const byte CTXValue = 0x25;

    /// <summary>
    /// Address array
    /// </summary>
    public static readonly byte[] AddressArray = new byte[] { 0xC1, 0xC2, 0xC3, 0xC4, 0xC5 };

    /// <summary>
    /// crypt data-array with addr and ctxvalue
    /// </summary>
    /// <param name="addr">address array</param>
    /// <param name="data">data array to encrypt</param>
    /// <param name="ctxValue">ctx value for encryption</param>
    /// <param name="rfPayload">crypted array</param>
    /// <returns>size of crypted array</returns>
    public static int GetRfPayload(byte[] addr, byte[] data, byte ctxValue, out byte[] rfPayload)
    {
        // resulting advertisment array has a length of constant 24 bytes
        const int rfPayloadLength = 24;

        int addrLength = addr.Length;
        int dataLength = data.Length;
        int lengthResultArray = addrLength + dataLength + 5;

        if (lengthResultArray > rfPayloadLength)
        {
            rfPayload = Array.Empty<byte>();
            return 0;
        }

        byte data_offset = 0x12;    // 0x12 (18)
        byte inverse_offset = 0x0f; // 0x0f (15)

        int result_data_size = data_offset + addrLength + dataLength + 2;
        byte[] resultbuf = new byte[result_data_size];

        resultbuf[15] = 0x71;   // 0x71 (113)
        resultbuf[16] = 0x0f;   // 0x0f (15)
        resultbuf[17] = 0x55;   // 0x55 (85)

        // copy firstDataArray reverse into targetArray with offset 18
        for (int index = 0; index < addrLength; index++)
        {
            resultbuf[index + data_offset] = addr[addrLength - 1 - index];
        }

        // copy dataArray into resultbuf with offset 18 + addrLength
        Buffer.BlockCopy(data, 0, resultbuf, data_offset + addrLength, dataLength);

        // crypt Bytes from position 15 to 22
        for (int index = inverse_offset; index < addrLength + data_offset; index++)
        {
            resultbuf[index] = CryptTools.Invert8(resultbuf[index]);
        }

        // calc checksum und copy to array
        ushort checksum = CryptTools.CheckCRC16(addr, data);
        resultbuf.SetUInt16(checksum, result_data_size - 2);

        byte[] ctx_0x3F = new byte[7]; // int local_58[8];
        CryptTools.WhiteningInit(0x3f, ctx_0x3F); // 0x3f (63) -> ctx_0x3F = [1111111]
        CryptTools.WhiteningEncode(resultbuf, 0x12, addrLength + dataLength + 2, ctx_0x3F);

        byte[] ctx = new byte[7];
        CryptTools.WhiteningInit(ctxValue, ctx); // ctxValue= 0x25 (37) -> ctx = [1101110]
        CryptTools.WhiteningEncode(resultbuf, 0, result_data_size, ctx);

        // resulting advertisment array has a length of constant 24 bytes
        rfPayload = new byte[rfPayloadLength];

        Buffer.BlockCopy(resultbuf, 15, rfPayload, 0, lengthResultArray);

        // fill rest of array
        for (int index = lengthResultArray; index < rfPayloadLength; index++)
        {
            rfPayload[index] = (byte)(index + 1);
        }

        return rfPayloadLength;
    }
}
