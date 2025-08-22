using System;

namespace BrickController2.Protocols;

/// <summary>
/// static class containing functions needed by encryption algorithm for the advertising data
/// </summary>
public static class CryptTools
{
    /// <summary>
    /// Generates an RF payload by combining and processing the provided seed, header, and data arrays.
    /// </summary>
    /// <remarks>The method combines the header, a reversed and inverted version of the seed, and the data
    /// array, appends a CRC16 checksum, and applies two levels of whitening using the provided context values. The
    /// resulting RF payload is written to the specified offset in the <paramref name="rfPayload"/> array.</remarks>
    /// <param name="seed">The seed array used for generating the RF payload. The array is reversed and processed during the operation.</param>
    /// <param name="header">The header array to include at the beginning of the RF payload.</param>
    /// <param name="data">The data array to include in the RF payload after the header and seed.</param>
    /// <param name="headerOffset">The offset in the RF payload where the header should be placed.</param>
    /// <param name="ctxValue1">The first context value used for whitening the seed, data, and checksum.</param>
    /// <param name="ctxValue2">The second context value used for whitening the entire RF payload.</param>
    /// <param name="rfPayload">The output array where the generated RF payload will be written.</param>
    /// <param name="rfPayloadOffset">The offset in the <paramref name="rfPayload"/> array where the RF payload should be written. Defaults to 0.</param>
    /// <returns>The total length of the generated RF payload written to <paramref name="rfPayload"/>, or 0 if the <paramref
    /// name="rfPayload"/> array does not have sufficient space to hold the result.</returns>
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

        int seedOffset = headerOffset + headerLength;
        int dataOffset = seedOffset + seedLength;
        int checksumOffset = dataOffset + dataLength;
        int resultBufferLength = checksumOffset + checksumLength;

        Span<byte> resultBuffer = stackalloc byte[resultBufferLength];

        // Copy header
        header.AsSpan().CopyTo(resultBuffer.Slice(headerOffset, headerLength));

        // Reverse-copy seed-array into resultBuffer after header
        for (int index = 0; index < seedLength; index++)
        {
            resultBuffer[seedOffset + index] = seed[seedLength - 1 - index];
        }

        // Invert bytes of header and seed-array in resultBuffer
        for (int index = 0; index < headerLength + seedLength; index++)
        {
            resultBuffer[headerOffset + index] = Reverse(resultBuffer[headerOffset + index]);
        }

        // Copy data
        data.AsSpan().CopyTo(resultBuffer.Slice(dataOffset, dataLength));

        // Write checksum
        ushort checksum = CheckCRC16(seed, data);
        if (!BitConverter.TryWriteBytes(resultBuffer.Slice(checksumOffset, checksumLength), checksum))
        {
            return 0;
        }

        // Whitening
        Span<byte> ctxArray1 = stackalloc byte[7];
        WhiteningInit(ctxValue1, ctxArray1);
        WhiteningEncode(resultBuffer, seedOffset, seedLength + dataLength + checksumLength, ctxArray1);

        Span<byte> ctxArray2 = stackalloc byte[7];
        WhiteningInit(ctxValue2, ctxArray2);
        WhiteningEncode(resultBuffer, 0, resultBufferLength, ctxArray2);

        // Copy result to rfPayload
        resultBuffer.Slice(headerOffset, resultArrayLength).CopyTo(rfPayload.AsSpan(rfPayloadOffset, resultArrayLength));

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
    /// Computes the CRC-16 checksum for the given input byte arrays using the CRC-16-CCITT algorithm.
    /// </summary>
    /// <remarks>This method processes the bytes in <paramref name="array1"/> in reverse order and the bytes
    /// in <paramref name="array2"/> in their original order. The CRC-16-CCITT algorithm is used with an initial value
    /// of 0xFFFF and a polynomial of 0x1021. The result is inverted and XORed with 0xFFFF before being
    /// returned.</remarks>
    /// <param name="array1">The first byte array to include in the CRC-16 calculation. Cannot be null.</param>
    /// <param name="array2">The second byte array to include in the CRC-16 calculation. Cannot be null.</param>
    /// <returns>A 16-bit unsigned integer representing the computed CRC-16 checksum.</returns>
    public static ushort CheckCRC16(byte[] array1, byte[] array2)
    {
        int result = 0xFFFF;

        // Process array1 in reverse order
        for (int i = array1.Length - 1; i >= 0; i--)
        {
            result ^= array1[i] << 8;
            for (int j = 0; j < 8; j++)
            {
                result = (result & 0x8000) == 0 ? result << 1 : (result << 1) ^ 0x1021;
            }
        }

        // Process array2 in forward order, with bit inversion
        for (int i = 0; i < array2.Length; i++)
        {
            result ^= Reverse(array2[i]) << 8;
            for (int j = 0; j < 8; j++)
            {
                result = (result & 0x8000) == 0 ? result << 1 : (result << 1) ^ 0x1021;
            }
        }

        // Final inversion and XOR
        return (ushort)(Reverse((ushort)result) ^ 0xFFFF);
    }
    
    /// <summary>
    /// Initializes a whitening context by extracting individual bits from the specified value.
    /// </summary>
    /// <remarks>The <paramref name="ctx"/> span must have a length of at least 7. If the span is smaller, 
    /// the method will throw an <see cref="IndexOutOfRangeException"/>.</remarks>
    /// <param name="val">The input byte value from which bits are extracted.</param>
    /// <param name="ctx">A span of bytes representing the whitening context. The first element is set to 1,  and the subsequent elements
    /// <param name="ctx">A span of bytes representing the whitening context. The first element is set to 1, and the subsequent elements
    /// (indices 1 through 6) are populated with the individual bits of <paramref name="val"/>, starting from the most
    /// significant bit (bit 5) to the least significant bit (bit 0).</param>
    public static void WhiteningInit(byte val, Span<byte> ctx)
    {
        ctx[0] = 1;
        ctx[1] = (byte)((val >> 5) & 1);
        ctx[2] = (byte)((val >> 4) & 1);
        ctx[3] = (byte)((val >> 3) & 1);
        ctx[4] = (byte)((val >> 2) & 1);
        ctx[5] = (byte)((val >> 1) & 1);
        ctx[6] = (byte)(val & 1);
    }

    /// <summary>
    /// Applies a whitening transformation to a specified segment of a byte array.
    /// </summary>
    /// <remarks>The whitening transformation modifies the specified segment of the <paramref name="data"/>
    /// array in-place by XORing each bit with a value derived from the <paramref name="ctx"/>. Ensure that the
    /// <paramref name="ctx"/> span is correctly initialized and that the range specified by <paramref
    /// name="dataStartIndex"/> and <paramref name="len"/> is valid within the <paramref name="data"/> array.</remarks>
    /// <param name="data">The byte array containing the data to be transformed. The transformation is applied in-place.</param>
    /// <param name="dataStartIndex">The starting index in the <paramref name="data"/> array where the transformation begins.</param>
    /// <param name="len">The number of bytes to transform, starting from <paramref name="dataStartIndex"/>.</param>
    /// <param name="ctx">A span of bytes representing the context used for the whitening transformation. This must be properly
    /// initialized before calling the method.</param>
    public static void WhiteningEncode(Span<byte> data, int dataStartIndex, int len, Span<byte> ctx)
    {
        for (int index = 0; index < len; index++)
        {
            byte currentByte = data[dataStartIndex + index];
            int currentResult = 0;
            for (int bitIndex = 0; bitIndex < 8; bitIndex++)
            {
                byte uVar2 = WhiteningOutput(ctx);
                currentResult |= ((uVar2 ^ ((currentByte >> bitIndex) & 1)) << bitIndex);
            }
            data[dataStartIndex + index] = (byte)currentResult;
        }
    }

    /// <summary>
    /// Performs a whitening operation on the provided context and returns the updated value at the first position.
    /// </summary>
    /// <remarks>This method modifies the input span in place by shifting and transforming its elements.  The
    /// operation involves a bitwise XOR between specific elements of the span, and the result is stored in the fourth
    /// position.</remarks>
    /// <param name="ctx">A span of bytes representing the context to be transformed. The span must contain at least 7 elements.</param>
    /// <returns>The byte value at the first position of the context after the whitening operation.</returns>
    private static byte WhiteningOutput(Span<byte> ctx)
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
