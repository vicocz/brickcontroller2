using System;

namespace BrickController2.DeviceManagement.Vengit;

/// <summary>
/// Contains implementation of SBrick protocol <see href="https://social.sbrick.com/custom/The_SBrick_BLE_Protocol.pdf"/>
/// </summary>
internal static class SBrickProtocol
{
    public static class Services
    {
        /// <summary>
        /// Remote Control service
        /// </summary>
        public static readonly Guid RemoteControl = new("4dc591b0-857c-41de-b5f1-15abda665b0c");
    }
    public static class Characteristics
    {
        /// <summary>
        /// Remote control service: Remote Control Commands characteristic UUID
        /// </summary>
        public static readonly Guid RemoteControlCommand = new("02b8cbcc-0e25-4bda-8790-a15f53e6010f");
    }

    // SBrick Light configuration
    public const byte LIGHT_PORTS_COUNT = 8;
    public const byte LIGHT_BANK_0_SIZE = 16;
    public const byte LIGHT_BANK_1_SIZE = 8;

    public const byte LIGHT_SUBCHANNEL_COUNT = 3;
    public const byte LIGHT_SUBCHANNEL_RED = 0;
    public const byte LIGHT_SUBCHANNEL_GREEN = 1;
    public const byte LIGHT_SUBCHANNEL_BLUE = 2;

    // Light flags
    public const byte LIGHTS_FLAGS_BANK_0 = 0x00;
    public const byte LIGHTS_FLAGS_BANK_1 = 0x01;
    public const byte LIGHTS_FLAGS_APPLY = 0x80;

    // data records
    public const byte DATA_RECORD_PRODUCT_TYPE = 0x00;

    public const byte PRODUCT_ID_SBRICK = 0x00;
    public const byte PRODUCT_ID_SBRICK_LIGHT = 0x01;
    public const byte PRODUCT_ID_UNKNOWN = 0xFF;

    // ADC channels
    public const byte ADC_CHANNEL_VOLTAGE = 0x08;

    // commands
    public const byte CMD_QUERY_ADC = 0x0F;
    public const byte CMD_SET_ALL_LIGHTS = 0x36;

    // message builders
    public static byte[] BuildSetAllLights(byte flags, ReadOnlySpan<byte> values)
    {
        // 0x36 Set all lights
        var buffer = new byte[2 + values.Length];

        buffer[0] = CMD_SET_ALL_LIGHTS;
        buffer[1] = flags;
        values.CopyTo(buffer.AsSpan(2));

        return buffer;
    }

    public static bool TryGetSBrickLightVoltage(byte[]? voltageData, out float voltage)
    {
        if (voltageData == null || voltageData.Length < 2)
        {
            voltage = default;
            return false;
        }

        var rawVoltage = voltageData[0] + (voltageData[1] << 8);
        voltage = (rawVoltage * 0.42567F) / 2047;
        return true;
    }
}
