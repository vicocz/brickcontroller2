using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.DeviceManagement.IO;
using BrickController2.Extensions;
using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using BrickController2.Settings;

using static BrickController2.DeviceManagement.Vengit.SBrickProtocol;

namespace BrickController2.DeviceManagement.Vengit;

internal class SBrickLightDevice : BluetoothDevice
{
    private const string ChannelASettingName = "ChannelAColor";
    private const string ChannelBSettingName = "ChannelBColor";
    private const string ChannelCSettingName = "ChannelCColor";
    private const string ChannelDSettingName = "ChannelDColor";
    private const string ChannelESettingName = "ChannelEColor";
    private const string ChannelFSettingName = "ChannelFColor";
    private const string ChannelGSettingName = "ChannelGColor";
    private const string ChannelHSettingName = "ChannelHColor";

    private const string ColorSettingGroupName = "ChannelColors";

    private static readonly RgbColor DEFAULT_CHANNEL_COLOR = new(r: 1.0f, g: 1.0f, b: 1.0f);

    private readonly OutputValuesGroup<byte> _bankOutputs0 = new(LIGHT_BANK_0_SIZE);
    private readonly OutputValuesGroup<byte> _bankOutputs1 = new(LIGHT_BANK_1_SIZE);

    private IGattCharacteristic? _firmwareRevisionCharacteristic;
    private IGattCharacteristic? _hardwareRevisionCharacteristic;
    private IGattCharacteristic? _remoteControlCharacteristic;

    public SBrickLightDevice(string name, string address, byte[] deviceData, IEnumerable<NamedSetting> settings, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
        : base(name, address, deviceRepository, bleService)
    {
        // apply A-H channel color settings
        SetSettingValue(ChannelASettingName, settings, ColorSettingGroupName, DEFAULT_CHANNEL_COLOR);
        SetSettingValue(ChannelBSettingName, settings, ColorSettingGroupName, DEFAULT_CHANNEL_COLOR);
        SetSettingValue(ChannelCSettingName, settings, ColorSettingGroupName, DEFAULT_CHANNEL_COLOR);
        SetSettingValue(ChannelDSettingName, settings, ColorSettingGroupName, DEFAULT_CHANNEL_COLOR);
        SetSettingValue(ChannelESettingName, settings, ColorSettingGroupName, DEFAULT_CHANNEL_COLOR);
        SetSettingValue(ChannelFSettingName, settings, ColorSettingGroupName, DEFAULT_CHANNEL_COLOR);
        SetSettingValue(ChannelGSettingName, settings, ColorSettingGroupName, DEFAULT_CHANNEL_COLOR);
        SetSettingValue(ChannelHSettingName, settings, ColorSettingGroupName, DEFAULT_CHANNEL_COLOR);
    }

    public override DeviceType DeviceType => DeviceType.SBrickLight;
    public override string BatteryVoltageSign => "V";

    public override int NumberOfChannels => LIGHT_PORTS_COUNT;
    protected override bool AutoConnectOnFirstConnect => false;

    public override void SetOutput(int channel, float value)
    {
        // normalize value to 0..1 as it's light, not speed
        value = CutOutputValue(Math.Abs(value));

        var port = channel % LIGHT_PORTS_COUNT;
        var baseChannel = LIGHT_SUBCHANNEL_COUNT * port;

        if (channel < LIGHT_PORTS_COUNT)
        {
            // get channel color and transform to HSV model to modify lightness
            var defaultColor = GetDefaultChannelColor(channel);
            var color = defaultColor.WithValueFactor(value);

            // each channel controls 3 subchannels-RGB
            SetChannelOutput(baseChannel + LIGHT_SUBCHANNEL_RED, color.R);
            SetChannelOutput(baseChannel + LIGHT_SUBCHANNEL_GREEN, color.G);
            SetChannelOutput(baseChannel + LIGHT_SUBCHANNEL_BLUE, color.B);
        }
        else
        {
            // write directly
            var subchannel = channel / LIGHT_PORTS_COUNT - 1;
            SetChannelOutput(baseChannel + subchannel, value);
        }
    }

    public RgbColor GetDefaultChannelColor(int channel)
    {
        var port = channel % LIGHT_PORTS_COUNT;
        string settingName = port switch
        {
            0 => ChannelASettingName,
            1 => ChannelBSettingName,
            2 => ChannelCSettingName,
            3 => ChannelDSettingName,
            4 => ChannelESettingName,
            5 => ChannelFSettingName,
            6 => ChannelGSettingName,
            7 => ChannelHSettingName,
            _ => throw new ArgumentOutOfRangeException(nameof(channel)),
        };
        return GetSettingValue(settingName, DEFAULT_CHANNEL_COLOR);
    }

    protected override Task<bool> ValidateServicesAsync(IEnumerable<IGattService>? services, CancellationToken token)
    {
        _firmwareRevisionCharacteristic = services.GetCharacteristic(GattProtocol.Services.DeviceInformation, GattProtocol.Characteristics.FirmwareRevision);
        _hardwareRevisionCharacteristic = services.GetCharacteristic(GattProtocol.Services.DeviceInformation, GattProtocol.Characteristics.HardwareRevision);
        _remoteControlCharacteristic = services.GetCharacteristic(Services.RemoteControl, Characteristics.RemoteControlCommand);

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

    private void SetChannelOutput(int index, float value)
    {
        // for lights use 0-255 range
        var rawValue = (byte)(value * 255);

        // address correct bank
        if (index < LIGHT_BANK_0_SIZE)
        {
            _bankOutputs0.SetOutput(index, rawValue);
        }
        else if (index < LIGHT_BANK_0_SIZE + LIGHT_BANK_1_SIZE)
        {
            _bankOutputs1.SetOutput(index - LIGHT_BANK_0_SIZE, rawValue);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(index));
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
