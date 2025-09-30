using System.Buffers.Binary;
using System;

namespace BrickController2.Protocols;

public static class BluetoothLowEnergy
{
    // advertisment data types
    public const byte ADTYPE_INCOMPLETE_SERVICE_128BIT = 0x06; // Service: Additional 128-bit UUIDs
    public const byte ADTYPE_COMPLETE_SERVICE_128BIT = 0x07;   // Service: Additional 128-bit UUIDs
    public const byte ADTYPE_LOCAL_NAME_COMPLETE = 0x09;   // Complete local name
    public const byte ADTYPE_MANUFACTURER_SPECIFIC = 0xFF; // Manufacturer specific data

    public static ushort GetUInt16(this ReadOnlySpan<byte> value, int index = 0)
        => BinaryPrimitives.ReadUInt16LittleEndian(value[index..]);

    public static short GetInt16(this ReadOnlySpan<byte> data, int index = 0)
        => BinaryPrimitives.ReadInt16LittleEndian(data[index..]);

    public static int GetInt32(this ReadOnlySpan<byte> data, int index = 0)
        => BinaryPrimitives.ReadInt32LittleEndian(data[index..]);

    public static Guid GetGuid(this ReadOnlySpan<byte> data, int index = 0)
    {
        return new Guid(
            BinaryPrimitives.ReadInt32LittleEndian(data[(index + 12)..]),
            BinaryPrimitives.ReadInt16LittleEndian(data[(index + 10)..]),
            BinaryPrimitives.ReadInt16LittleEndian(data[(index + 8)..]),
            data[index + 7], data[index + 6], data[index + 5], data[index + 4], data[index + 3], data[index + 2], data[index + 1], data[index]);
    }

    public static byte[] To128BitByteArray(this Guid guid)
    {
        Span<byte> guidBytes = stackalloc byte[16];
        guid.TryWriteBytes(guidBytes);

        // Rearrange the bytes to match the Bluetooth Low Energy specification
        guidBytes.Reverse();
        guidBytes.Slice(8, 2).Reverse();
        guidBytes.Slice(10, 2).Reverse();
        guidBytes.Slice(12, 4).Reverse();

        return guidBytes.ToArray();
    }
}
