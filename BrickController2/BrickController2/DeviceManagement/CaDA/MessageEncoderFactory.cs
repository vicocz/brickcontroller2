using System;

namespace BrickController2.DeviceManagement.CaDA;

public class MessageEncoderFactory : IMessageEncoderFactory
{
    private readonly ICaDADeviceManager _cadaManager;
    private readonly Random _random;

    public MessageEncoderFactory(ICaDADeviceManager cadaManager, Random random)
    {
        _cadaManager = cadaManager;
        _random = random;
    }

    /// <summary>
    /// Create a new message encoder for the specified device.
    /// </summary>
    /// <param name="deviceData">Device data</param>
    /// <returns>New message encoder instance</returns>
    public IMessageEncoder Create(ReadOnlySpan<byte> deviceData)
    {
        // CADA RaceCar - Rev2
        if (deviceData.Length == 16)
        {
            // device id (bytes 5 & 6) for later use in payload template
            return new RaceCarMessageEncoderRev2(deviceId: deviceData.Slice(5, 2),
                // AppId 16bits
                appId: _cadaManager.GetAppId().Span.Slice(0, 2),
                defaultSequenceValue: deviceData[11]);
        }
        // CADA RaceCar - original one
        if (deviceData.Length == 18)
        {
            // DeviceData-Array is the manufacturer specific data inside the response telegram sent when
            // * scanning for the device.
            // * loading the device from database

            // It's containing:
            // * DeviceAddress of the real CaDA device
            // * AppID sent from this App on scanning
            // These values are patched into the DataArray wich is advertised to control the device.

            return new RaceCarMessageEncoder(
                _random,
                deviceAddress: deviceData[4..6],
                appId: deviceData[7..9]);
        }

        // fallback
        throw new InvalidOperationException("Unsupported device data format.");
    }
}