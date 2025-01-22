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

    public const byte MOTOR_ACTION_STOP = 0x10;
    public const byte MOTOR_ACTION_STOP_AB = 0x10 | MOTOR_OUTPUT_AB;
    public const byte MOTOR_ACTION_SET_SPD = 0x70;
    public const byte MOTOR_ACTION_SET_SPD_AB = 0x70 | MOTOR_OUTPUT_AB;

    public const byte MOTOR_OUTPUT_A = 0x01;
    public const byte MOTOR_OUTPUT_B = 0x02;
    public const byte MOTOR_OUTPUT_AB = MOTOR_OUTPUT_A | MOTOR_OUTPUT_B;

    public const byte MOTOR_SPEED_MASK = 0x3F;
    public const byte MOTOR_SPEED_FLAG_HIRES_REV = 0x40;
    public const byte MOTOR_SPEED_FLAG_HIRES = 0x80;

    public static byte[] SetMotorSpeed(int v0, int v1)
    {
        var motorParam1 = GetMotorParam(v0);
        var motorParam2 = GetMotorParam(v1);
        var motorActionId = v0 == 0 && v1 == 0 ? MOTOR_ACTION_STOP_AB : MOTOR_ACTION_SET_SPD_AB;

        return [CMD_PRE_DELIMITER, CMD_PRE_DELIMITER, CMD_PRE_DELIMITER,
            CMD_TEST_ACTION,
            EVT_COMMAND_NONE, // command
            motorActionId, // Byte 1 - MOTOR_ACTION_ID / MOTOR_MASK
            motorParam1, // Byte 2 - MOTOR_PARAM1
            motorParam2, // Byte 3 - MOTOR_PARAM2
            0x00, // lightFxId;
            0x00, // lightOutputMask;
            0x00, // lightPFOutputMask;
            0x00, // lightParam1;
            0x00, // lightParam2;
            0x00, // lightParam3;
            0x00, // lightParam4;
            0x00, // lightParam5;
            0x00, // soundFxId;
            0x00, // soundFileId;
            0x00, // soundParam1;
            0x00, // soundParam2;
            CMD_POST_DELIMITER, CMD_POST_DELIMITER, CMD_POST_DELIMITER];
    }

    public static byte[] AllOff()
    {
        return [CMD_PRE_DELIMITER, CMD_PRE_DELIMITER, CMD_PRE_DELIMITER,
            CMD_TEST_ACTION,
            EVT_COMMAND_ALL_OFF, // command
            0x00, // Byte 1 - MOTOR_ACTION_ID / MOTOR_MASK
            0x00, // Byte 2 - MOTOR_PARAM1
            0x00, // Byte 3 - MOTOR_PARAM2
            0x00, // lightFxId;
            0x00, // lightOutputMask;
            0x00, // lightPFOutputMask;
            0x00, // lightParam1;
            0x00, // lightParam2;
            0x00, // lightParam3;
            0x00, // lightParam4;
            0x00, // lightParam5;
            0x00, // soundFxId;
            0x00, // soundFileId;
            0x00, // soundParam1;
            0x00, // soundParam2;
            CMD_POST_DELIMITER, CMD_POST_DELIMITER, CMD_POST_DELIMITER];
    }

    public static byte[] GetStatus()
    {
        return [CMD_PRE_DELIMITER, CMD_PRE_DELIMITER, CMD_PRE_DELIMITER,
            CMD_GET_STATUS, // command;
            0xA5, // PFX_STATUS_BYTE0;
            0x5A, // PFX_STATUS_BYTE1;
            0x6E, // PFX_STATUS_BYTE2;
            0x40, // PFX_STATUS_BYTE3;
            0x54, // PFX_STATUS_BYTE4;
            0xA4, // PFX_STATUS_BYTE5;
            0xE5, // PFX_STATUS_BYTE6;
            CMD_POST_DELIMITER, CMD_POST_DELIMITER, CMD_POST_DELIMITER];
    }

    private static byte GetMotorParam(int speed)
    {
        var value = MOTOR_SPEED_FLAG_HIRES | (Math.Abs(speed * 63 / 100) & MOTOR_SPEED_MASK);

        return speed < 0 ? (byte)(value | MOTOR_SPEED_FLAG_HIRES_REV) : (byte)value;
    }
}
