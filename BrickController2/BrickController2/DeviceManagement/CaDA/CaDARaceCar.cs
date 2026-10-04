using System;
using BrickController2.DeviceManagement.IO;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;

namespace BrickController2.DeviceManagement.CaDA;

/// <summary>
/// CaDA RaceCar
/// </summary>
internal class CaDARaceCar : BluetoothAdvertisingDevice
{
    private readonly IMessageEncoder _messageEncoder;
    private readonly OutputValuesGroup<Half> _outputValues = new(4);

    public CaDARaceCar(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IMessageEncoderFactory messageEncoderFactory)
      : base(name, address, deviceRepository, bleService)
    {
        // create message encoder for this device based on advertised data
        _messageEncoder = messageEncoderFactory.Create(deviceData);
    }
    public override DeviceType DeviceType => DeviceType.CaDA_RaceCar;

    /// <summary>
    /// manufacturerId to advertise
    /// </summary>
    protected override ushort ManufacturerId => CaDAProtocol.ManufacturerID;

    public override int NumberOfChannels => 4;

    public override void SetOutput(int channelNo, float value)
    {
        CheckChannel(channelNo);
        value = CutOutputValue(value);

        // check for change
        if (SetChannelOutput(channelNo, value))
        {
            // notify data changed
            _bluetoothAdvertisingDeviceHandler.NotifyDataChanged();
        }
    }

    protected override void InitDevice()
    {
        _outputValues.Initialize();
        _messageEncoder.Initialize();
    }

    protected override void DisconnectDevice()
    {
    }

    protected bool TryGetTelegram(bool getConnectTelegram, out byte[] currentData)
    {
        var changed = _outputValues.TryGetValues(out var outputValues);
        currentData = _messageEncoder.Encode(outputValues, getConnectTelegram);

        return changed || getConnectTelegram;
    }

    /// <summary>
    /// Get or create BluetoothAdvertisingDeviceHandler
    /// </summary>
    /// <returns>Instance of BluetoothAdvertisingDeviceHandler</returns>
    protected override BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler()
    {
        return new BluetoothAdvertisingDeviceHandler(_bleService, ManufacturerId, TryGetTelegram, TimeSpan.MaxValue);
    }

    private bool SetChannelOutput(int channelNo, float value)
    {
        return channelNo switch
        {
            2 or 3 => _outputValues.SetOutput(channelNo, (Math.Abs(value) > 0.5f) ? Half.One : Half.Zero),  // channel 2 front lights, channel 3 rear lights
            _ => _outputValues.SetOutput(channelNo, (Half)value)                                            // channel 0 throttle, channel 1 steering
        };
    }
}
