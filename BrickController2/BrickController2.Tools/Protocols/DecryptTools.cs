using System;
using BrickController2.Helpers;
using BrickController2.Protocols;

namespace BrickController2.Tools.Protocols;

/// <summary>
/// static class containing functions to decrypt advertising data
/// </summary>
public static class DecryptTools
{
    /// <summary>
    /// Decrypts an RF payload and extracts the original data, verifying its integrity using a CRC checksum.
    /// </summary>
    /// <remarks>This method reverses the encoding applied to an RF payload, including whitening and inversion
    /// operations, and validates the integrity of the extracted data using a CRC16 checksum. The caller must provide
    /// the necessary parameters, including the seed, header length, data length, offsets, and context values, which are
    /// used to decode the payload correctly.  If the CRC validation fails, an <see cref="InvalidOperationException"/>
    /// is thrown, indicating that the data may be corrupted or the provided parameters are incorrect.</remarks>
    /// <param name="seed">The seed array used for decoding the payload. Cannot be null or empty.</param>
    /// <param name="headerLength">The length of the header in bytes. Must be non-negative.</param>
    /// <param name="dataLength">The length of the data to extract in bytes. Must be non-negative.</param>
    /// <param name="headerOffset">The offset in the payload where the header begins. Must be non-negative.</param>
    /// <param name="ctxValue1">The first context value used for whitening operations.</param>
    /// <param name="ctxValue2">The second context value used for whitening operations.</param>
    /// <param name="rfPayload">The RF payload to decrypt. Cannot be null and must contain sufficient data based on the provided parameters.</param>
    /// <returns>A byte array containing the decrypted data extracted from the RF payload. The array will contain <paramref
    /// name="dataLength"/> bytes of original data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the CRC validation fails, indicating that the data may be corrupted or the provided parameters are
    /// incorrect.</exception>
    public static byte[] DecryptRfPayload(byte[] seed, int headerLength, int dataLength, int headerOffset, byte ctxValue1, byte ctxValue2, byte[] rfPayload)
    {
        const int checksumLength = 2;
        int seedLength = seed.Length;
        int resultArrayLength = headerLength + seedLength + dataLength + checksumLength;

        int seedOffset = headerOffset + headerLength;
        int dataOffset = seedOffset + seedLength;
        int checksumOffset = dataOffset + dataLength;

        int resultBufferLength = checksumOffset + checksumLength;

        // Prepare buffer and copy payload into the correct offset
        byte[] resultBuffer = new byte[resultBufferLength];
        Buffer.BlockCopy(rfPayload, 0, resultBuffer, headerOffset, resultArrayLength);

        // Reverse WhiteningEncode with ctxValue2 (same as encode, since it's XOR-based)
        byte[] ctxArray2 = new byte[7];
        CryptTools.WhiteningInit(ctxValue2, ctxArray2);
        CryptTools.WhiteningEncode(resultBuffer, 0, resultBufferLength, ctxArray2);

        // Reverse WhiteningEncode with ctxValue1 (same as encode, since it's XOR-based)
        byte[] ctxArray1 = new byte[7];
        CryptTools.WhiteningInit(ctxValue1, ctxArray1);
        CryptTools.WhiteningEncode(resultBuffer, seedOffset, seedLength + dataLength + checksumLength, ctxArray1);

        // Extract and invert header+seed
        byte[] header = new byte[headerLength];
        byte[] seedReversed = new byte[seedLength];
        for (int i = 0; i < headerLength; i++)
        {
            header[i] = CryptTools.Invert8(resultBuffer[headerOffset + i]);
        }

        for (int i = 0; i < seedLength; i++)
        {
            seedReversed[i] = CryptTools.Invert8(resultBuffer[headerOffset + headerLength + i]);
        }

        // Reverse the seed array to get the original order
        byte[] seedOriginal = new byte[seedLength];
        for (int i = 0; i < seedLength; i++)
        {
            seedOriginal[i] = seedReversed[seedLength - 1 - i];
        }

        // Extract data
        byte[] data = new byte[dataLength];
        Buffer.BlockCopy(resultBuffer, dataOffset, data, 0, dataLength);

        // Optionally, verify checksum
        ushort expectedCrc = resultBuffer.GetUInt16(checksumOffset);
        ushort actualCrc = CryptTools.CheckCRC16(seedOriginal, data);

        if (expectedCrc != actualCrc)
        {
            throw new InvalidOperationException("CRC check failed. Data may be corrupted or parameters are incorrect.");
        }

        // Return the original data array
        return data;
    }
}
