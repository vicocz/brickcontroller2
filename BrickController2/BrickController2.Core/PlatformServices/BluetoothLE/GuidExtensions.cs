using System.Buffers.Binary;

namespace BrickController2.Core.PlatformServices.BluetoothLE;

public static class GuidExtensions
{
    public static string ToBluetoothAddress(this Guid guid)
    {
        byte[] bytes = guid.ToByteArray();

        // Check if the first 10 bytes are all 0. 
        // This identifies a GUID that is likely a masked MAC address.
        bool isMacFormat = bytes.Take(10).All(b => b == 0);

        if (isMacFormat)
        {
            // Extract the last 6 bytes and format as MAC (00:1A:...)
            return string.Join(":", bytes.Skip(10)
                .Select(b => b.ToString("X2")));
        }

        // Fallback: If it's a random/native GUID (like on iOS), return standard string
        return guid.ToString();
    }

    public static bool TryParseBluetoothAddressToGuid(this string stringValue, out Guid guid)
    {
        guid = Guid.Empty;

        // Standard MAC format is 17 chars: XX:XX:XX:XX:XX:XX
        if (string.IsNullOrEmpty(stringValue) || stringValue.Length != 17)
        {
            return false;
        }

        ulong value = 0;

        for (int i = 1; i <= stringValue.Length; i++)
        {
            var ch = (uint)stringValue[i - 1];
            if (i % 3 == 0)
            {
                if (ch != '-' && ch != ':') return false;
            }
            else
            {
                uint nibble;
                if (ch >= 0x30 && ch <= 0x39) nibble = ch - 0x30;        // 0-9
                else if (ch >= 0x41 && ch <= 0x46) nibble = ch - 0x37;   // A-F
                else if (ch >= 0x61 && ch <= 0x66) nibble = ch - 0x57;   // a-f (added lowercase support)
                else return false;

                value = (value << 4) + nibble;
            }
        }

        // Allocate 16 bytes on the stack
        Span<byte> guidBytes = stackalloc byte[16];
        guidBytes.Clear(); // Ensure first 10 bytes are 0

        // Write the ulong as Big Endian into the last 8 bytes of the span
        // Because MAC is 48-bit, the bytes will land in indices 10 through 15.
        BinaryPrimitives.WriteUInt64BigEndian(guidBytes[8..], value);

        guid = new Guid(guidBytes);
        return true;
    }
}
