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

        public static void SetInt32(this byte[] data, int offset, int value)
        {
            data[offset + 0] = (byte)(value & 0xff);
            data[offset + 1] = (byte)((value >> 8) & 0xff);
            data[offset + 2] = (byte)((value >> 16) & 0xff);
            data[offset + 3] = (byte)((value >> 24) & 0xff);
        }

        public static short GetInt16(this byte[] data, int offset)
        {
            return (short)(data[offset] |
                (data[offset + 1] << 8));
        }

        public static int GetInt32(this byte[] data, int offset)
        {
            return data[offset] |
                (data[offset + 1] << 8) |
                (data[offset + 2] << 16) |
                (data[offset + 3] << 24);
        }
    }
}
