using System;
using System.Collections.Generic;
using System.Text;

namespace BrickController2.Helpers
{
    public static class ByteArrayExtensions
    {
        public static string ToAsciiStringSafe(this byte[] data)
        {
            try
            {
                if (data != null)
                {
                    return Encoding.ASCII.GetString(data);
                }
            }
            catch
            {
            }

            return null;
        }

        public static ushort ReadUInt16LE(this IReadOnlyList<byte> data, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt16(new byte[] { data[startIndex], data[startIndex + 1] }, 0);
            }

            return BitConverter.ToUInt16(new byte[] { data[startIndex + 1], data[startIndex] }, 0);
        }
    }
}
