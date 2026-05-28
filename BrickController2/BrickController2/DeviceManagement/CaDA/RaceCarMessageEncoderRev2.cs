using System;

using static BrickController2.Protocols.CaDAProtocol;

namespace BrickController2.DeviceManagement.CaDA;

public class RaceCarMessageEncoderRev2 : IMessageEncoder
{
    private readonly ICaDAPlatformService _platformService;
    private readonly byte _defaultSequenceValue;

    private readonly byte[] _data; // 16 bytes total
    private byte _sequence;

    internal RaceCarMessageEncoderRev2(ICaDAPlatformService platformService,
        ReadOnlySpan<byte> deviceId,
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

        _platformService = platformService;

        // init sequence counter
        _defaultSequenceValue = defaultSequenceValue;
        _sequence = defaultSequenceValue;

        // prepare 16bytes of payload data
        _data =
        [
            // header (pairing/command marker, 0xAA/0xBB) + constant/manufacturer/protocol byte
            0xAA, 0x11,
            // product/model identifier (CaDA RaceCar)
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

    /// <inheritdoc/>
    public void Initialize()
    {
        // reset sequence counter to default value
        _sequence = _defaultSequenceValue;
    }

    /// <inheritdoc/>
    public byte[] Encode(ReadOnlySpan<Half> values, bool connectDevice = false)
    {
        // check params
        if (values.Length != 3)
        {
            throw new ArgumentException("Invalid input data.", nameof(values));
        }

        EncodeValues(values, connectDevice);

        // finally use the platform service to encrypt the whole payload
        _platformService.TryGetRfPayloadV2(_data, out var rfPayload);

        return rfPayload;
    }

    internal ReadOnlySpan<byte> EncodeValues(ReadOnlySpan<Half> values, bool connectDevice = false)
    {
        // Map input (-1.0 to 1.0) to Throttle (0xFF to 0x00)
        byte throttle = Clamp(HalfByte - (values[0] * HalfByte));
        // Map input (-1.0 to 1.0) to Steering (0x00 to 0xFF)
        byte steering = Clamp(HalfByte + (values[1] * HalfByte));

        // header: PAIRING : COMMAND
        var header = connectDevice ? (byte)0xAA : (byte)0xBB;

        // update payload with current control data
        _data[0] = header; // header
        _data[7] = throttle;
        _data[8] = steering;
        // flags: lights on/off
        _data[9] = MapAsFlag(values[2]);
        _data[10] = 0x00; // reset checksum before recalculating
        _data[11] = (connectDevice || (throttle == 0x80 && steering == 0x80))
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
