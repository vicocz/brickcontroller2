using System;

using static System.Half; // for convenient usage of Half constants like 0.5h

namespace BrickController2.DeviceManagement.CaDA;

public class RaceCarMessageEncoderRev2 : IMessageEncoder
{
    private readonly byte _defaultSequenceValue;

    private readonly byte[] _data; // 16 bytes total
    private byte _sequence;

    internal RaceCarMessageEncoderRev2(ReadOnlySpan<byte> deviceId,
        ReadOnlySpan<byte> appId,
        byte defaultSequenceValue)
    {
        if (deviceId.Length != 2)
        {
            throw new ArgumentException("DeviceId must be 2 bytes.", nameof(deviceId));
        }
        if (appId.Length != 2)
        {
            throw new ArgumentException("AppId must be 2 bytes.", nameof(appId));
        }

        // init sequence counter
        _defaultSequenceValue = defaultSequenceValue;
        _sequence = defaultSequenceValue;

        // prepare 16bytes of payload data
        _data =
        [
            // manufacturerId
            0xAA, 0x11,
            // CADA RaceCar?
            0x11,
            // DeviceId
            0x00, 0x00,
            // 2 bytes AppID - zeros from the scan
            0x00, 0x00,
            // throttle, steering, lights
            0x00, 0x00, 0x00,
            // checksum, sequence
            0x00, _sequence,
            // 4 bytes footer, ending with 0xA0 / 0xB0
            0xCC, 0xB8, 0x92, 0xA0
        ];

        deviceId.CopyTo(_data.AsSpan(3)); // DeviceId at index 3-4
        appId.CopyTo(_data.AsSpan(5)); // AppID at index 5-6
    }

    internal RaceCarMessageEncoderRev2(ushort deviceId, ushort appId, byte defaultSequenceValue)
    {
        // init sequence counter
        _defaultSequenceValue = defaultSequenceValue;
        _sequence = defaultSequenceValue;

        // prepare 16bytes of payload data
        _data = 
        [
            // manufacturerId
            0xAA, 0x11,
            // CADA RaceCar?
            0x11,
            // DeviceId
            (byte)(deviceId & 0xFF), (byte)((deviceId >> 8) & 0xFF),
            // 2 bytes AppID - zeros from the scan
            (byte)(appId & 0xFF), (byte)((appId >> 8) & 0xFF),
            // throttle, steering, lights
            0x00, 0x00, 0x00,
            // checksum, sequence
            0x00, _sequence,
            // 4 bytes footer, ending with 0xA0 / 0xB0
            0xCC, 0xB8, 0x92, 0xA0
        ];
    }

    public void Initialize()
    {
        // reset sequence counter to default value
        _sequence = _defaultSequenceValue;
    }

    /// <summary>
    /// Encode the control data to a byte array which can be sent to the device.
    /// </summary>connect = false
    /// <param name="controlData">Control data to encode</param>
    /// <returns>Encoded byte array</returns>
    public ReadOnlySpan<byte> Encode(ReadOnlySpan<Half> values, bool connect = false)
    {
        // check params
        if (values.Length != 3)
        {
            throw new ArgumentException("Invalid inpur data.", nameof(values));
        }

        var oneHalf = (Half)0.5f;
        var halfByte = (Half)128.0f;
        var maxByteHalf = (Half)255.0f;

        // Map input (-1.0 to 1.0) to Throttle (0xFF to 0x00)
        byte throttle = (byte)Clamp(halfByte - (values[0] * halfByte), Zero, maxByteHalf);
        // Map input (-1.0 to 1.0) to Steering (0x00 to 0xFF)
        byte steering = (byte)Clamp(halfByte + (values[1] * halfByte), Zero, maxByteHalf);

        // header: PAIRING : COMMAND
        var header = connect ? (byte)0xAA : (byte)0xBB;

        // update payload with current control data
        _data[0] = header; // header
        _data[7] = throttle;
        _data[8] = steering;
        // flags: lights on/off
        _data[9] = (byte)(Abs(values[2]) > oneHalf ? 0x01 : 0x00);
        _data[10] = 0x00; // reset checksum before recalculating
        _data[11] = (connect || (throttle == 0x80 && steering == 0x80))
            ? _defaultSequenceValue
            : ++_sequence;
        _data[15] = (byte)(header & 0xF0); // footer

        // The checksum is the sum of all 16 UNENCRYPTED bytes mod 256.
        int checksum = 0;
        for (int i = 0; i < _data.Length; i++)
        {
            checksum += _data[i];
        }
        _data[10] = (byte)checksum;
        // Encrypt the throttle/steering via Bitwise XOR (Bytes 7 & 8)
        _data[7] ^= _data[10];
        _data[8] ^= _data[10];

        return _data;
    }
}
