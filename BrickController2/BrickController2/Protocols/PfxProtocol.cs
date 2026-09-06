using System;
using System.Buffers.Binary;
using System.Text;

namespace BrickController2.Protocols;

internal static class PfxProtocol
{
    public const byte CMD_FILE_DIR = 0x45;
    public const byte RSP_FILE_DIR = 0xC5; // CMD_FILE_DIR | 0x80 (response/ack opcode)
    public const byte CMD_PRE_DELIMITER = 0x5B;
    public const byte CMD_POST_DELIMITER = 0x5D;

    // File directory request codes
    public const byte PFX_DIR_REQ_GET_FILE_COUNT = 0x00;
    public const byte PFX_DIR_REQ_GET_FREE_SPACE = 0x01;
    public const byte PFX_DIR_REQ_GET_DIR_ENTRY_IDX = 0x02;
    public const byte PFX_DIR_REQ_GET_DIR_ENTRY_ID = 0x03;
    public const byte PFX_DIR_REQ_GET_NAMED_FILE_ID = 0x0B;

    public const byte CMD_GET_STATUS = 0x01;
    public const byte CMD_TEST_ACTION = 0x13;

    public const byte EVT_COMMAND_NONE = 0x00;
    public const byte EVT_COMMAND_ALL_OFF = 0x01;

    // Motor Action IDs
    public const byte MOTOR_ACTION_EMERGENCY_STOP = 0x00;
    public const byte MOTOR_ACTION_STOP = 0x10;
    public const byte MOTOR_ACTION_STOP_ALL = 0x10 | MOTOR_OUTPUT_ALL;
    public const byte MOTOR_ACTION_SET_SPD = 0x70;
    public const byte MOTOR_ACTION_SET_SPD_ALL = 0x70 | MOTOR_OUTPUT_ALL;

    public const byte MOTOR_OUTPUT_BASE = 0x01;
    public const byte MOTOR_OUTPUT_MASK = 0x0F;
    public const byte MOTOR_OUTPUT_A = 0x01;
    public const byte MOTOR_OUTPUT_B = 0x02;
    public const byte MOTOR_OUTPUT_ALL = MOTOR_OUTPUT_A | MOTOR_OUTPUT_B;

    public const byte MOTOR_SPEED_MASK = 0x3F;
    public const byte MOTOR_SPEED_FLAG_HIRES_REV = 0x40;
    public const byte MOTOR_SPEED_FLAG_HIRES = 0x80;

    // Light FX IDs
    public const byte EVT_LIGHTFX_ON_OFF_TOGGLE = 0x01;
    public const byte EVT_LIGHTFX_SET_BRIGHTNESS = 0x04;

    public const byte LIGHT_OUTPUT_BASE = 0x01;
    public const byte LIGHT_OUTPUT_1 = 0x01;
    public const byte LIGHT_OUTPUT_2 = 0x02;
    public const byte LIGHT_OUTPUT_3 = 0x04;
    public const byte LIGHT_OUTPUT_4 = 0x08;
    public const byte LIGHT_OUTPUT_5 = 0x10;
    public const byte LIGHT_OUTPUT_6 = 0x20;
    public const byte LIGHT_OUTPUT_7 = 0x40;
    public const byte LIGHT_OUTPUT_8 = 0x80;

    public const byte LIGHT_OUTPUT_ALL = LIGHT_OUTPUT_1 |
        LIGHT_OUTPUT_2 |
        LIGHT_OUTPUT_3 |
        LIGHT_OUTPUT_4 |
        LIGHT_OUTPUT_5 |
        LIGHT_OUTPUT_6 |
        LIGHT_OUTPUT_7 |
        LIGHT_OUTPUT_8;

    public const byte EVT_LIGHTFX_TRANSITION_TOGGLE = 0x00;
    public const byte EVT_LIGHTFX_TRANSITION_ON = 0x01;
    public const byte EVT_LIGHTFX_TRANSITION_OFF = 0x02;

    // Sound FX IDs (SOUND_FX_ID, section 6.2.14)
    public const byte EVT_SOUNDFX_NONE = 0x00;
    public const byte EVT_SOUNDFX_INC_VOLUME = 0x01;
    public const byte EVT_SOUNDFX_DEC_VOLUME = 0x02;
    public const byte EVT_SOUNDFX_SET_VOLUME = 0x03;
    public const byte EVT_SOUNDFX_PLAY_ONCE = 0x04;
    public const byte EVT_SOUNDFX_PLAY_CONTINUOUS = 0x05;
    public const byte EVT_SOUNDFX_PLAY_NTIMES = 0x06;
    public const byte EVT_SOUNDFX_PLAY_DURATION = 0x07;
    public const byte EVT_SOUNDFX_STOP = 0x0B;

    // EVT_SOUNDFX_PLAY_ONCE / RETRIGGER (SOUND_PARAM1)
    public const byte EVT_SOUNDFX_RETRIGGER_TOGGLE = 0x00;
    public const byte EVT_SOUNDFX_RETRIGGER_RESTART = 0x01;

    /// <summary>
    /// Set speed of the selected <paramref name="motorOutput"/> channel
    /// </summary>
    public static byte[] SetMotorSpeed(int channel, short speed)
        => SetMotorSpeed(channel == int.MaxValue ? MOTOR_OUTPUT_ALL : (byte)(MOTOR_OUTPUT_BASE << channel), speed);

    /// <summary>
    /// Set speed of the provided <paramref name="motorOutput"/> channel bit mask
    /// </summary>
    public static byte[] SetMotorSpeed(byte motorOutput, short speed)
        => TestEventAction(EVT_COMMAND_NONE,
            motorActionId: (byte)(MOTOR_ACTION_SET_SPD | motorOutput & MOTOR_OUTPUT_MASK),
            motorParam1: GetMotorParam(speed));

    /// <summary>
    /// Turn off all motors, lights, and sound.
    /// </summary>
    public static byte[] AllOff() => TestEventAction(EVT_COMMAND_ALL_OFF);

    /// <summary>
    /// Set the brightness of the selected <paramref name="lightChannel"/> channel
    /// </summary>
    public static byte[] SetBrightness(int lightChannel, short value)
        => SetBrightness(lightChannel == int.MaxValue ? LIGHT_OUTPUT_ALL : (byte)(LIGHT_OUTPUT_BASE << lightChannel), (byte)(Math.Abs(value) & 0xFF));

    /// <summary>
    /// Set the brightness of the provided <paramref name="lightChannel"/> channel bit mask
    /// </summary>
    public static byte[] SetBrightness(byte lightOutputMask, byte value)
        => TestEventAction(EVT_COMMAND_NONE,
            lightFxId: EVT_LIGHTFX_SET_BRIGHTNESS,
            lightOutputMask: lightOutputMask,
            lightParam1: value);

    /// <summary>
    /// Set the light of the selected <paramref name="lightChannel"/> ON / OFF based on the provided value
    /// </summary>
    public static byte[] SetLight(int lightChannel, short value)
        => SetLight(lightChannel == int.MaxValue ? LIGHT_OUTPUT_ALL : (byte)(LIGHT_OUTPUT_BASE << lightChannel), value == 0 ? EVT_LIGHTFX_TRANSITION_OFF : EVT_LIGHTFX_TRANSITION_ON);

    /// <summary>
    /// Set the light of selected <paramref name="lightChannel"/> channel bit mask ON / OFF
    /// </summary>
    public static byte[] SetLight(byte lightOutputMask, byte value)
        => TestEventAction(EVT_COMMAND_NONE,
            lightFxId: EVT_LIGHTFX_ON_OFF_TOGGLE,
            lightOutputMask: lightOutputMask,
            lightParam4: value);

    /// <summary>
    /// Request the total number of files stored on the PFx Brick file system.
    /// </summary>
    public static byte[] GetFileCount() => [CMD_PRE_DELIMITER, CMD_PRE_DELIMITER, CMD_PRE_DELIMITER,
        CMD_FILE_DIR, PFX_DIR_REQ_GET_FILE_COUNT,
        CMD_POST_DELIMITER, CMD_POST_DELIMITER, CMD_POST_DELIMITER];

    /// <summary>
    /// Request the directory entry (file id, size, name, ...) at the given index.
    /// </summary>
    public static byte[] GetDirEntryAtIndex(byte index) => [CMD_PRE_DELIMITER, CMD_PRE_DELIMITER, CMD_PRE_DELIMITER,
        CMD_FILE_DIR, PFX_DIR_REQ_GET_DIR_ENTRY_IDX, index,
        CMD_POST_DELIMITER, CMD_POST_DELIMITER, CMD_POST_DELIMITER];

    /// <summary>
    /// Parses a "Get Directory Entry" response (request 0x02 / 0x03) into a <see cref="PfxFileDirEntry"/>.
    /// </summary>
    /// <remarks>
    /// Confirmed against a real device response: fields are big-endian, and the layout matches the ICD
    /// exactly (no extra echoed request-code byte, unlike the "Get File Count" response). An entry with
    /// <see cref="FileDirEntry.FirstSector"/> == 0xFFFF is an empty/unused directory slot.
    /// </remarks>
    public static FileDirEntry? ParseFileDirEntry(byte[] data)
    {
        const int FixedFieldsLength = 24;
        const int MaxNameLength = 32;

        if (data.Length < FixedFieldsLength || data[0] != RSP_FILE_DIR)
        {
            return null;
        }

        var fileId = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(2, 2));
        var fileSize = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(4, 4));
        var firstSector = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(8, 2));
        var attributes = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(10, 2));
        var userData1 = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(12, 4));
        var userData2 = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(16, 4));
        var crc32 = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(20, 4));

        var nameLength = Math.Min(MaxNameLength, data.Length - FixedFieldsLength);
        var name = nameLength > 0
            ? Encoding.UTF8.GetString(data, FixedFieldsLength, nameLength).TrimEnd('\0', ' ')
            : string.Empty;

        return new FileDirEntry(fileId, fileSize, firstSector, attributes, userData1, userData2, crc32, name);
    }

    /// <summary>
    /// Parses a "Get File Count" response.
    /// </summary>
    /// <remarks>
    /// The ICD documents a 4-byte response: [0xC5, RequestStatus, FileCount[15:0] (big-endian)].
    /// Observed device responses are 5 bytes: [0xC5, RequestStatus, 0x00, FileCount[15:0] (big-endian)],
    /// with an extra byte at offset 2 that appears to echo the request sub-code (0x00 for GET_FILE_COUNT).
    /// Reading the count from the last 2 bytes handles both layouts.
    /// </remarks>
    public static ushort? ParseFileCount(byte[] data)
    {
        if (data.Length < 4 || data[0] != RSP_FILE_DIR)
        {
            return null;
        }

        return BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(data.Length - 2, 2));
    }

    /// <summary>
    /// Plays the sound file identified by <paramref name="fileId"/> a single time.
    /// </summary>
    /// <param name="fileId">PFx Brick file id (0-255).</param>
    /// <param name="retrigger">
    /// Behavior if the file is already playing when triggered again:
    /// <see cref="SOUNDFX_RETRIGGER_TOGGLE"/> (toggle on/off) or <see cref="SOUNDFX_RETRIGGER_RESTART"/> (restart from beginning).
    /// Defaults to restart, which is usually the expected behavior for a macro trigger.
    /// </param>
    /// <param name="relativeVolume">
    /// Complement relative volume (dB gain/attenuation) applied from the current playback volume. Range: -8..7. Defaults to 0 (no change).
    /// </param>
    public static byte[] PlaySoundFile(byte fileId, byte retrigger = EVT_SOUNDFX_RETRIGGER_RESTART, sbyte relativeVolume = 0)
        => TestEventAction(EVT_COMMAND_NONE,
            soundFxId: EVT_SOUNDFX_PLAY_ONCE,
            soundFileId: fileId,
            soundParam1: retrigger,
            soundParam2: unchecked((byte)relativeVolume));

    /// <summary>
    /// Stops playback of the sound file identified by <paramref name="fileId"/>.
    /// </summary>
    public static byte[] StopSoundFile(byte fileId)
        => TestEventAction(EVT_COMMAND_NONE,
            soundFxId: EVT_SOUNDFX_STOP,
            soundFileId: fileId);

    /// <summary>
    /// Sets the playback volume.
    /// </summary>
    public static byte[] SetVolume(float volume)
        => TestEventAction(EVT_COMMAND_NONE,
            soundFxId: EVT_SOUNDFX_SET_VOLUME,
            soundParam1: volume switch
            {
                < 0.0f => 0,
                > 100.0f => 255,
                _ => (byte)(volume / 100f * 255.0f)
            });

    public static byte[] IncreaseVolume() => TestEventAction(EVT_COMMAND_NONE, soundFxId: EVT_SOUNDFX_INC_VOLUME);
    public static byte[] DecreaseVolume() => TestEventAction(EVT_COMMAND_NONE, soundFxId: EVT_SOUNDFX_DEC_VOLUME);

    /// <summary>
    /// Get the status of the device.
    /// </summary>
    public static byte[] GetStatus() => [CMD_PRE_DELIMITER, CMD_PRE_DELIMITER, CMD_PRE_DELIMITER,
        CMD_GET_STATUS, // command;
        0xA5, // PFX_STATUS_BYTE0;
        0x5A, // PFX_STATUS_BYTE1;
        0x6E, // PFX_STATUS_BYTE2;
        0x40, // PFX_STATUS_BYTE3;
        0x54, // PFX_STATUS_BYTE4;
        0xA4, // PFX_STATUS_BYTE5;
        0xE5, // PFX_STATUS_BYTE6;
        CMD_POST_DELIMITER, CMD_POST_DELIMITER, CMD_POST_DELIMITER];

    /// <summary>
    /// Trigger command test
    /// </summary>
    public static byte[] TestEventAction(byte command,
        byte motorActionId = 0x00,
        byte motorParam1 = 0x00,
        byte motorParam2 = 0x00,
        byte lightFxId = 0x00,
        byte lightOutputMask = 0x00,
        byte lightParam1 = 0x00,
        byte lightParam2 = 0x00,
        byte lightParam3 = 0x00,
        byte lightParam4 = 0x00,
        byte soundFxId = 0x00,
        byte soundFileId = 0x00,
        byte soundParam1 = 0x00,
        byte soundParam2 = 0x00)
        => [CMD_PRE_DELIMITER, CMD_PRE_DELIMITER, CMD_PRE_DELIMITER,
            CMD_TEST_ACTION,
            command,          // command
            motorActionId,    // Byte 1 - MOTOR_ACTION_ID / MOTOR_MASK
            motorParam1,      // Byte 2 - MOTOR_PARAM1
            motorParam2,      // Byte 3 - MOTOR_PARAM2 - duration
            lightFxId,        // lightFxId;
            lightOutputMask,  // lightOutputMask;
            0x00,             // lightPFOutputMask;
            lightParam1,      // lightParam1;
            lightParam2,      // lightParam2;
            lightParam3,      // lightParam3;
            lightParam4,      // lightParam4;
            0x00,             // lightParam5;
            soundFxId,        // soundFxId;
            soundFileId,      // soundFileId;
            soundParam1,      // soundParam1;
            soundParam2,      // soundParam2;
            CMD_POST_DELIMITER, CMD_POST_DELIMITER, CMD_POST_DELIMITER];

    private static byte GetMotorParam(int speed)
    {
        var value = MOTOR_SPEED_FLAG_HIRES | (Math.Abs(speed * 63 / 100) & MOTOR_SPEED_MASK);

        return speed < 0 ? (byte)(value | MOTOR_SPEED_FLAG_HIRES_REV) : (byte)value;
    }

    internal enum FileFormat : byte
    {
        Wav = 0x00,
        Flac = 0x01,
        Mp3 = 0x02,
        Ogg = 0x03,
        Au = 0x04,
        Gsm = 0x05,
        Txt = 0x10,
        Hex = 0x11,
        Zip = 0x20,
        Gz = 0x21,
        Pfx = 0x30,
        Img = 0x50,
    }

    internal readonly record struct FileDirEntry(ushort FileId,
        uint FileSize,
        ushort FirstSector,
        ushort Attributes,
        uint UserData1,
        uint UserData2,
        uint Crc32,
        string FileName)
    {
        /// <summary>
        /// File format code stored in User Attributes[15:8] (ICD 9.1.5).
        /// </summary>
        public FileFormat FileFormat => (FileFormat)(Attributes >> 8);

        public bool IsValid => FirstSector != 0xFFFF && !string.IsNullOrEmpty(FileName);

        public bool IsAudio => FileFormat <= FileFormat.Gsm;
    }
}
