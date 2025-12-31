using System;

namespace BrickController2.DeviceManagement.Vengit;

/// <summary>
/// Contains implementation of SBrick protocol <see href="https://social.sbrick.com/custom/The_SBrick_BLE_Protocol.pdf"/>
/// </summary>
internal static class SBrickProtocol
{
    /// <summary>
    /// SBrick - Remote control service UUID
    /// </summary>
    public static readonly Guid ServiceUuid = new("4dc591b0-857c-41de-b5f1-15abda665b0c");
    /// <summary>
    /// Remote control service - Remote control commands characteristic UUID
    /// </summary>
    public static readonly Guid RemoteControlCharacteristicUuid = new("02b8cbcc-0e25-4bda-8790-a15f53e6010f");

    // Light flags
    public const byte LIGHTS_FLAGS_BANK_0 = 0x00;
    public const byte LIGHTS_FLAGS_BANK_1 = 0x01;
    public const byte LIGHTS_FLAGS_APPLY = 0x80;

    // message builders
    public static byte[] BuildSetAllLights(byte flags, ReadOnlySpan<byte> values)
    {
        // 0x36 Set all lights
        var buffer = new byte[2 + values.Length];

        buffer[0] = 0x36;
        buffer[1] = flags;
        values.CopyTo(buffer.AsSpan(2));

        return buffer;
    }
}
