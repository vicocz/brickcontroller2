using BrickController2.DeviceManagement.IO;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement;

internal class PfxBrickDevice : BluetoothDevice
{
    private const int PF_CHANNELS = 2;
    private const int LIGHT_CHANNELS = 8;

    private static readonly Guid SERVICE_UUID = new("49535343-fe7d-4ae5-8fa9-9fafd205e455");
    private static readonly Guid CHARACTERISTIC_UUID_WRITE = new("49535343-8841-43f4-a8d4-ecbe34729bb3");
    private static readonly Guid CHARACTERISTIC_UUID_NOTIFY = new("49535343-1e4d-4bd9-ba61-23c647249616");

    private readonly OutputValuesGroup<short> _motorOutputs = new(PF_CHANNELS);
    private readonly OutputValuesGroup<short> _lightOutputs = new(LIGHT_CHANNELS);

    private IGattCharacteristic? _writeCharacteristic;
    private IGattCharacteristic? _notifyCharacteristic;

    public PfxBrickDevice(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
        : base(name, address, deviceRepository, bleService)
    {
    }

    public override DeviceType DeviceType => DeviceType.PfxBrick;

    public override int NumberOfChannels => 10;

    protected override bool AutoConnectOnFirstConnect => false;

    public override void SetOutput(int channel, float value)
    {
        CheckChannel(channel);
        value = CutOutputValue(value);

        if (channel >= PF_CHANNELS)
        {
            // Per light channel range: +- [0 .. 255]
            var brightnessValue = (short)(value * 255);
            int lightChannel = channel - PF_CHANNELS;
            _lightOutputs.SetOutput(lightChannel, brightnessValue);
        }
        else
        {
            // Per motor channel range: +- percent
            var percentValue = (short)(value * 100);
            _motorOutputs.SetOutput(channel, percentValue);
        }
    }

    protected override async Task<bool> ValidateServicesAsync(IEnumerable<IGattService>? services, CancellationToken token)
    {
        var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
        _writeCharacteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_WRITE);

        _notifyCharacteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_NOTIFY);
        if (_notifyCharacteristic is not null)
        {
            await _bleDevice!.EnableNotificationAsync(_notifyCharacteristic, token);
        }

        return _writeCharacteristic is not null;
    }

    protected override void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
    {
        if (characteristicGuid != _notifyCharacteristic!.Uuid || data.Length == 0)
            return;

        if (data.Length == 1) // notification
        {
        }
        else if (data.Length == 48) // status
        {
            HardwareVersion = $"{data[7]:X2}{data[8]:X2}"; // product_id
            FirmwareVersion = $"{data[37]:x2}.{data[38]:x2}"; // firmware_ver
        }
    }

    protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
    {
        try
        {
            if (requestDeviceInformation)
            {
                await ReadDeviceInfo(token);
            }
        }
        catch { }

        return true;
    }

    protected override async Task ProcessOutputsAsync(CancellationToken token)
    {
        try
        {
            // reset outputs
            _motorOutputs.Initialize();
            _lightOutputs.Initialize();

            while (!token.IsCancellationRequested)
            {
                bool changed = false;
                // process motor outputs for a change
                if (_motorOutputs.TryGetChanges(out var motorChanges))
                {
                    if (await SendOutputValuesAsync(motorChanges, token).ConfigureAwait(false))
                    {
                        // confirm successfull sending
                        _motorOutputs.Commmit();
                        await Task.Delay(5, token).ConfigureAwait(false);
                    }
                    changed = true;
                }

                // process light outputs for a change
                if (_lightOutputs.TryGetChanges(out var lightChanges))
                {
                    if (await SendLightValuesAsync(lightChanges, token).ConfigureAwait(false))
                    {
                        // confirm successfull sending
                        _lightOutputs.Commmit();
                        await Task.Delay(5, token).ConfigureAwait(false);
                    }
                    changed = true;
                }

                if (!changed)
                {
                    await Task.Delay(10, token).ConfigureAwait(false);
                }
            }

            // ensure everything is stopped in the end
            await WriteCommandAsync(PfxProtocol.AllOff(), token).ConfigureAwait(false);
        }
        catch
        {
        }
    }

    private async Task<bool> SendOutputValuesAsync(IEnumerable<KeyValuePair<int, short>> changes, CancellationToken token)
    {
        bool result = true;
        foreach (var change in changes)
        {
            var cmd = PfxProtocol.SetMotorSpeed(change.Key, change.Value);
            result &= await WriteCommandAsync(cmd, token);
        }
        return result;
    }

    private async Task<bool> SendLightValuesAsync(IEnumerable<KeyValuePair<int, short>> changes, CancellationToken token)
    {            
        bool result = true;
        foreach (var change in changes)
        {
            // apply brightness (if needed)
            if (change.Value != 0)
            {
                var cmd = PfxProtocol.SetBrightness(change.Key, change.Value);
                result &= await WriteCommandAsync(cmd, token);
            }
            // apply toggle ON / OFF based on value
            var toggleCmd = PfxProtocol.SetLight(change.Key, change.Value);
            result &= await WriteCommandAsync(toggleCmd, token);
        }
        return result;
    }

    private async Task<bool> WriteCommandAsync(byte[] command, CancellationToken token)
    {
        try
        {
            return await _bleDevice!.WriteAsync(_writeCharacteristic!, command, token);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task ReadDeviceInfo(CancellationToken token)
    {
        // request status update
        await _bleDevice!.WriteAsync(_writeCharacteristic!, PfxProtocol.GetStatus(), token);
    }
}
