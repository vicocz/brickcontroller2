using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.DeviceManagement.IO;
using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using static BrickController2.DeviceManagement.Vengit.SBrickProtocol;

namespace BrickController2.DeviceManagement.Vengit;

internal class SBrickLightDevice : BluetoothDevice
{
    private readonly OutputValuesGroup<byte> _bankOutputs0 = new(LIGHT_BANK_0_SIZE);
    private readonly OutputValuesGroup<byte> _bankOutputs1 = new(LIGHT_BANK_1_SIZE);

    private IGattCharacteristic? _firmwareRevisionCharacteristic;
    private IGattCharacteristic? _hardwareRevisionCharacteristic;
    private IGattCharacteristic? _remoteControlCharacteristic;

    public SBrickLightDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
        : base(name, address, deviceRepository, bleService)
    {
    }

    public override DeviceType DeviceType => DeviceType.SBrickLight;
    public override string BatteryVoltageSign => "V";

    /// <summary>
    /// Publish both
    /// - channels
    /// - subchannels
    /// </summary>
    public override int NumberOfChannels => LIGHT_PORTS_COUNT + LIGHT_BANK_0_SIZE + LIGHT_BANK_1_SIZE;
    protected override bool AutoConnectOnFirstConnect => false;

    public override void SetOutput(int channel, float value)
    {
        CheckChannel(channel);
        value = CutOutputValue(value);

        // for lights use 0-255 range
        var rawValue = (byte)(Math.Abs(value) * 255);

        if (channel < LIGHT_PORTS_COUNT)
        {
            // each channel controls 3 subchannels
            var subchannel = 3 * channel;
            SetOutput(subchannel + 0, rawValue);
            SetOutput(subchannel + 1, rawValue);
            SetOutput(subchannel + 2, rawValue);
        }
        else
        {
            // write directly subchannel
            var subchannel = channel - LIGHT_PORTS_COUNT;
            SetOutput(subchannel, rawValue);
        }
    }

    protected override Task<bool> ValidateServicesAsync(IEnumerable<IGattService>? services, CancellationToken token)
    {
        var deviceInformationService = services?.FirstOrDefault(s => s.Uuid == GattProtocol.Services.DeviceInformation);
        _firmwareRevisionCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == GattProtocol.Characteristics.FirmwareRevision);
        _hardwareRevisionCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == GattProtocol.Characteristics.HardwareRevision);

        var remoteControlService = services?.FirstOrDefault(s => s.Uuid == Services.RemoteControl);
        _remoteControlCharacteristic = remoteControlService?.Characteristics?.FirstOrDefault(c => c.Uuid == Characteristics.RemoteControlCommand);

        return Task.FromResult(
            _firmwareRevisionCharacteristic is not null &&
            _hardwareRevisionCharacteristic is not null &&
            _remoteControlCharacteristic is not null);
    }

    protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
    {
        try
        {
            if (requestDeviceInformation)
            {
                await ReadDeviceInfo(token).ConfigureAwait(false);
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
            _bankOutputs0.Initialize();
            _bankOutputs1.Initialize();

            while (!token.IsCancellationRequested)
            {
                // process first bank 0
                bool changed = await TryProcessChanges(_bankOutputs0, LIGHTS_FLAGS_APPLY | LIGHTS_FLAGS_BANK_0, token);

                // process additional bank 1
                if (await TryProcessChanges(_bankOutputs1, LIGHTS_FLAGS_APPLY | LIGHTS_FLAGS_BANK_1, token))
                {
                    changed = true;
                }

                if (!changed)
                {
                    await Task.Delay(10, token).ConfigureAwait(false);
                }
            }
        }
        catch
        {
        }
    }

    private void SetOutput(int subchannel, byte value)
    {
        if (subchannel < LIGHT_BANK_0_SIZE)
        {
            _bankOutputs0.SetOutput(subchannel, value);
        }
        else if (subchannel < LIGHT_BANK_0_SIZE + LIGHT_BANK_1_SIZE)
        {
            _bankOutputs1.SetOutput(subchannel - LIGHT_BANK_0_SIZE, value);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(subchannel));
        }
    }

    private async Task<bool> TryProcessChanges(OutputValuesGroup<byte> valueBank, byte flags, CancellationToken token)
    {
        try
        {
            if (valueBank.TryGetValues(out var values))
            {
                var command = BuildSetAllLights(flags, values);
                var success = await _bleDevice!.WriteAsync(_remoteControlCharacteristic!, command, token).ConfigureAwait(false);
                if (success)
                {
                    // confirm successful sending
                    valueBank.Commmit();
                    await Task.Delay(5, token).ConfigureAwait(false);
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task ReadDeviceInfo(CancellationToken token)
    {
        var firmwareData = await _bleDevice!.ReadAsync(_firmwareRevisionCharacteristic!, token);
        var firmwareVersion = firmwareData?.ToAsciiStringSafe();
        if (!string.IsNullOrEmpty(firmwareVersion))
        {
            FirmwareVersion = firmwareVersion;
        }

        var hardwareData = await _bleDevice.ReadAsync(_hardwareRevisionCharacteristic!, token);
        var hardwareVersion = hardwareData?.ToAsciiStringSafe();
        if (!string.IsNullOrEmpty(hardwareVersion))
        {
            HardwareVersion = hardwareVersion;
        }

        // 0x0F Query ADC | voltage on 0x08
        await _bleDevice.WriteAsync(_remoteControlCharacteristic!, [CMD_QUERY_ADC, ADC_CHANNEL_VOLTAGE], token);
        var voltageData = await _bleDevice!.ReadAsync(_remoteControlCharacteristic!, token);
        if (SBrickProtocol.TryGetSBrickLightVoltage(voltageData, out var voltage))
        {
            BatteryVoltage = voltage.ToString("F2");
        }
    }
}
