using BrickController2.Protocols;
using System;

using static System.Half;

namespace BrickController2.DeviceManagement.CaDA;

public class RaceCarMessageEncoder : IMessageEncoder
{
    private readonly ICaDAPlatformService _platformService;
    private readonly Random _random;

    /// <summary>
    /// 16 bytes including DeviceAddress, AppID and channelData
    /// </summary>
    private readonly byte[] _controlDataArray =
    [
            0x75, //  [0] const 0x75 (117)
            0x13, //  [1] 0x13 (19) STATUS_CONTROL
            0x00, //  [2] DeviceAddress
            0x00, //  [3] DeviceAddress
            0x00, //  [4] DeviceAddress
            0x00, //  [5] AppID
            0x00, //  [6] AppID
            0x00, //  [7] AppID
            0x00, //  [8] ChannelData random
            0x00, //  [9] ChannelData random
            0x80, // [10] ChannelData verticalValue (min= 0x80 (128))
            0x80, // [11] ChannelData horizontalValue (min= 0x80 (128))
            0x00, // [12] ChannelData lightValue
            0x00, // [13] ChannelData 
            0x00, // [14] ChannelData 
            0x00, // [15] ChannelData 
    ];

    internal RaceCarMessageEncoder(ICaDAPlatformService platformService,
        Random random,
        ReadOnlySpan<byte> deviceAddress,
        ReadOnlySpan<byte> appId)
    {
        _platformService = platformService;
        _random = random;

        // Configure encoder based on device data which contains:
        // * DeviceAddress of the real CaDA device
        // * AppID sent from this App on scanning
        // These values are patched into the DataArray wich is advertised to control the device.
        deviceAddress.CopyTo(_controlDataArray.AsSpan(2)); // DeviceAddress at index 2-4
        appId.CopyTo(_controlDataArray.AsSpan(5)); // AppID at index 5-7
    }

    public void Initialize()
    {
        // nothing to do in this encoder
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
            throw new ArgumentException("Invalid input data.", nameof(values));
        }

        // encode and encrypt the control data
        var encodedValues = EncodeValues(values);
        CaDAProtocol.Encrypt(encodedValues);

        // finally use the platform service to encrypt the whole payload
        _platformService.TryGetRfPayload(_controlDataArray, out var rfPayload);

        return rfPayload;
    }

    internal Span<byte> EncodeValues(ReadOnlySpan<Half> values)
    {
        var halfByte = (Half)128.0f;
        var maxByteHalf = (Half)255.0f;

        ushort random = (ushort)_random.Next(ushort.MinValue, ushort.MaxValue);

        _controlDataArray[8] = (byte)(random & 0xFF);
        _controlDataArray[9] = (byte)((random >> 8) & 0xFF);
        _controlDataArray[10] = (byte)Max(Zero, Min(halfByte - (values[0] * halfByte), maxByteHalf)); // speed value - reversed
        _controlDataArray[11] = (byte)Max(Zero, Min(halfByte + (values[1] * halfByte), maxByteHalf)); // 
        _controlDataArray[12] = (byte)Max(Zero, Min(halfByte + (values[2] * halfByte), maxByteHalf)); // light on/off
        _controlDataArray[13] = 0;
        _controlDataArray[14] = 0;
        _controlDataArray[15] = 0;

        // pin to encoded data for encryption
        return _controlDataArray.AsSpan(8);
    }
}
