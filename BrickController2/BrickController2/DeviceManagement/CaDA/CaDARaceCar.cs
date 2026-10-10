using System;
using System.Collections.Generic;
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
    private readonly OutputValuesGroup<Half> _outputValues;

    public CaDARaceCar(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IMessageEncoderFactory messageEncoderFactory)
      : base(name, address, deviceRepository, bleService)
    {
        _outputValues = new OutputValuesGroup<Half>(NumberOfChannels, _outputLock); // create with lock to ensure thread safety and data consistency when updating output values

        // create message encoder for this device based on advertised data
        _messageEncoder = messageEncoderFactory.Create(deviceData);
    }
    public override DeviceType DeviceType => DeviceType.CaDA_RaceCar;

    /// <summary>
    /// manufacturerId to advertise
    /// </summary>
    protected override ushort ManufacturerId => CaDAProtocol.ManufacturerID;

    public override int NumberOfChannels => 4;

    public override void SetOutputs(IEnumerable<(int, float)> outputs)
    {
        bool anyValueChanged = false;

        lock (_outputLock) // lock _outputValues to ensure thread safety and data consistency when updating output values
        {
            foreach (var (channel, value) in outputs)
            {
                CheckChannel(channel);
                float cutValue = CutOutputValue(value);

                // check for change
                anyValueChanged |= channel switch
                {
                    2 or 3 => _outputValues.SetOutput(channel, (Math.Abs(cutValue) > 0.5f) ? Half.One : Half.Zero),  // channel 2 front lights, channel 3 rear lights
                    _ => _outputValues.SetOutput(channel, (Half)cutValue)                                            // channel 0 throttle, channel 1 steering
                };
            }
        }

        if (anyValueChanged)
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
}
