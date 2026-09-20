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
    private readonly OutputValuesGroup<Half> _outputValues = new(3);
    private byte _lightsBitArray;


    public CaDARaceCar(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IMessageEncoderFactory messageEncoderFactory)
      : base(name, address, deviceData, deviceRepository, bleService)
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
        _lightsBitArray = 0;
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
            2 => SetLights(0, value), // front lights
            3 => SetLights(1, value), // rear lights
            _ => _outputValues.SetOutput(channelNo, (Half)value) // channels 0 and 1 are for motors
        };
    }

    private bool SetLights(int bitoffset, float value)
    {
        if (value == 0)
        {
            _lightsBitArray &= (byte)~(1 << bitoffset);
        }
        else
        {
            _lightsBitArray |= (byte)(1 << bitoffset);
        }
        return _outputValues.SetOutput(2, (Half)_lightsBitArray);
    }

}