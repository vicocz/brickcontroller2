using System;
using System.Buffers.Binary;

namespace BrickController2.Protocols;

/// <summary>
/// Contains implementation of Lego Wireless Protocol <see href="https://lego.github.io/lego-ble-wireless-protocol-docs/"/>
/// Inspired by <see href="https://github.com/toorisrael/LEGO-Porsche-Controller/blob/main/utils/lwp3_definitions.py"/>
/// </summary>
internal static class LegoWirelessProtocol
{


    // port modes
    public const byte PORT_MODE_0 = 0x00;
    public const byte PORT_MODE_1 = 0x01;
    public const byte PORT_MODE_2 = 0x02;
    public const byte PORT_MODE_3 = 0x03;
    public const byte PORT_MODE_4 = 0x04;

    // output command
    public const byte PORT_OUTPUT_COMMAND = 0x81;

    public const byte PORT_OUTPUT_SUBCOMMAND_WRITE_DIRECT = 0x51;

    // input command (single)
    public const byte PORT_INPUT_COMMAND = 0x41;

    public const byte PORT_VALUE_NOTIFICATION_DISABLED = 0x00;
    public const byte PORT_VALUE_NOTIFICATION_ENABLED = 0x01;

    public const byte FEEDBACK_ACTION_NO_ACTION = 0x00;
    public const byte FEEDBACK_ACTION_ACTION_COMPLETION = 0x01;
    public const byte FEEDBACK_ACTION_ACTION_START = 0x10;
    public const byte FEEDBACK_ACTION_BOTH = 0x11;

    // conversion methods
    public static void ToBytes(int value, out byte b0, out byte b1, out byte b2, out byte b3)
    {
        b0 = (byte)(value & 0xff);
        b1 = (byte)((value >> 8) & 0xff);
        b2 = (byte)((value >> 16) & 0xff);
        b3 = (byte)((value >> 24) & 0xff);
    }

    public static byte ToByte(int value) => (byte)(value & 0xFF);

    public static short ToInt16(byte[] value, int startIndex) => ToInt16(value.AsSpan(startIndex));
    public static int ToInt32(byte[] value, int startIndex) => ToInt32(value.AsSpan(startIndex));

    public static short ToInt16(ReadOnlySpan<byte> value) => BinaryPrimitives.ReadInt16LittleEndian(value);
    public static int ToInt32(ReadOnlySpan<byte> value) => BinaryPrimitives.ReadInt32LittleEndian(value);

    // message builder
    public static byte[] BuildPortInputFormatSetup(byte portId, byte portMode, int interval = 2, byte notification = PORT_VALUE_NOTIFICATION_ENABLED)
    {
        // Message Type - Port Input Format Setup (Single) [0x41]
        ToBytes(interval, out var i0, out var i1, out var i2, out var i3);
        return [0x0a, 0x00, PORT_INPUT_COMMAND, portId, portMode, i0, i1, i2, i3, notification];
    }
}
