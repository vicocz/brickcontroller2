using System;

namespace BrickController2.Protocols;

internal static class PfxProtocol
{
    public const byte CMD_PRE_DELIMITER = 0x5B;
    public const byte CMD_POST_DELIMITER = 0x5D;

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
        byte lightParam4 = 0x00)
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
            0x00,             // soundFxId;
            0x00,             // soundFileId;
            0x00,             // soundParam1;
            0x00,             // soundParam2;
            CMD_POST_DELIMITER, CMD_POST_DELIMITER, CMD_POST_DELIMITER];

    private static byte GetMotorParam(int speed)
    {
        var value = MOTOR_SPEED_FLAG_HIRES | (Math.Abs(speed * 63 / 100) & MOTOR_SPEED_MASK);

        return speed < 0 ? (byte)(value | MOTOR_SPEED_FLAG_HIRES_REV) : (byte)value;
    }
}
