using BrickController2.CreationManagement;
using BrickController2.DeviceManagement.IO;
using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static BrickController2.CreationManagement.ControllerDefaults;
using static BrickController2.Diagnostics.Logs;
using static BrickController2.Helpers.Await;
using static BrickController2.Protocols.LegoWirelessProtocol;

namespace BrickController2.DeviceManagement.Lego;

internal abstract class WirelessProtocolBasedDevice : BluetoothDevice
{
    protected readonly StateStore<int, ChannelConfig> ChannelConfigs;
    protected readonly ChannelPositionStore ChannelAbsPositions;
    protected readonly ChannelPositionStore ChannelRelativePositions;
    protected readonly StateStore<int, PeripheralAttachmentInfo> AttachedPeripherals;

    protected IGattCharacteristic? Characteristic;

    protected WirelessProtocolBasedDevice(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
     : base(name, address, deviceRepository, bleService)
    {
        ChannelConfigs = new();
        ChannelAbsPositions = new();
        ChannelRelativePositions = new();
        AttachedPeripherals = new(PeripheralAttachmentInfo.Initial);
    }

    public override string BatteryVoltageSign => "%";
    protected override bool AutoConnectOnFirstConnect => false;

    public override Task<DeviceConnectionResult> ConnectAsync(
        bool reconnect,
        Action<Device> onDeviceDisconnected,
        IEnumerable<ChannelConfiguration> channelConfigurations,
        bool startOutputProcessing,
        bool requestDeviceInformation,
        CancellationToken token)
    {
        // reset output values & positions
        ResetOutputValues();
        ChannelConfigs.Clear();

        // Initialize configuration per channel

        // Build dictionary, but for supported ones only
        var configs = new Dictionary<int, ChannelConfiguration>();
        foreach (var channelConfiguration in channelConfigurations.Where(c => IsOutputTypeSupported(c.Channel, c.ChannelOutputType)))
        {
            // If multiple configurations target the same channel, keep the last one.
            configs[channelConfiguration.Channel] = channelConfiguration;
        }

        for (int i = 0; i < NumberOfChannels; i++)
        {
            configs.TryGetValue(i, out var config);
            ChannelConfigs.Set(i, ChannelConfig.From(config));
        }

        return base.ConnectAsync(reconnect, onDeviceDisconnected, channelConfigurations, startOutputProcessing, requestDeviceInformation, token);
    }

    protected virtual byte GetPortId(int channelIndex) => (byte)channelIndex;
    protected virtual bool TryGetChannelIndex(byte portId, out int channelIndex)
    {
        channelIndex = portId;
        return portId < NumberOfChannels;
    }

    protected virtual void ResetOutputValues()
    {
        // reset status values & positions
        ChannelAbsPositions.Clear();
        ChannelRelativePositions.Clear();
        AttachedPeripherals.Clear();
    }

    protected override async Task ProcessOutputsAsync(CancellationToken token)
    {
        try
        {
            // initialize
            ResetOutputValues();

            while (!token.IsCancellationRequested)
            {
                if (!await SendOutputValuesAsync(token).ConfigureAwait(false))
                {
                    await Task.Delay(10, token).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            Dump("ProcessOutputsAsync: EXCEPTION", ex);
        }
    }

    protected abstract Task<bool> SendOutputValuesAsync(CancellationToken token);

    protected override async Task<bool> ValidateServicesAsync(IEnumerable<IGattService>? services, CancellationToken token)
    {
        var service = services?.FirstOrDefault(s => s.Uuid == ServiceUuid);
        Characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CharacteristicUuid);

        if (Characteristic is not null)
        {
            return await _bleDevice!.EnableNotificationAsync(Characteristic, token);
        }

        return false;
    }

    protected override async ValueTask BeforeDisconnectAsync(CancellationToken token)
    {
        // reset notifications (if possible)
        if (Characteristic != null && _bleDevice != null)
        {
            await _bleDevice.DisableNotificationAsync(Characteristic, token);
        }
    }

    protected override void BeforeDisconnectCleanup()
    {
        Characteristic = null;
    }

    protected override void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
    {
        if (data.Length < 4 || characteristicGuid != Characteristic?.Uuid)
        {
            return;
        }

        TryProcessMessageData(messageType: data[2], data);
    }

    protected virtual bool TryProcessMessageData(byte messageType, ReadOnlySpan<byte> data)
    {
        switch (messageType)
        {
            case MESSAGE_TYPE_HUB_PROPERTIES: // Hub properties
                ProcessHubPropertyData(data);
                return true;

            case MESSAGE_TYPE_PORT_VALUE: // Port value (single mode)
                {
                    if (!TryGetChannelIndex(portId: data[3], out var channel))
                    {
                        break;
                    }
                    if (data.Length == 6)
                    {
                        // assume 16bit data is ABS
                        var absPosition = ToInt16(data.Slice(4));
                        ChannelAbsPositions.Update(channel, pos => pos.WithPosition(absPosition));
                    }
                    else if (data.Length == 8)
                    {
                        // assume 32 bit data is REL
                        var relPosition = ToInt32(data.Slice(4));
                        ChannelRelativePositions.Update(channel, pos => pos.WithPosition(relPosition));
                    }
                    Dump("PORT_VALUE", data);
                    return true;
                }

            case MESSAGE_TYPE_PORT_VALUE_COMBINED: // Port value (combined mode)
                {
                    if (data.Length < 6 ||
                        !TryGetChannelIndex(portId: data[3], out var channel))
                    {
                        break;
                    }

                    var modeMask = data[5];
                    var currentData = data.Slice(6); // start at index 6

                    if ((modeMask & 0x01) != 0)
                    {
                        var absPosition = ToInt16(currentData);
                        ChannelAbsPositions.Update(channel, pos => pos.WithPosition(absPosition));

                        currentData = currentData.Slice(2);
                    }

                    if ((modeMask & 0x02) != 0)
                    {
                        // TODO: Read the port value format response and determine the value length accordingly
                        int relPosition = currentData.Length switch
                        {
                            >= 4 => ToInt32(currentData),
                            >= 2 => ToInt16(currentData),
                            _ => currentData[0]
                        };
                        ChannelRelativePositions.Update(channel, pos => pos.WithPosition(relPosition));
                    }
                    Dump("PORT_VALUE_COMBINED", data);
                    return true;
                }
            case MESSAGE_TYPE_HUB_ATTACHED_IO: // Hub attached I/O
                {
                    Dump("HUB_ATTACHED_IO", data);

                    if (TryGetChannelIndex(portId: data[3], out var channel))
                    {
                        byte eventType = data[4];

                        if (eventType == HUB_EVENT_ATTACHED || eventType == HUB_EVENT_ATTACHED_VIRTUAL)
                        {
                            // Get Unique type identification of the attached I/O device
                            var deviceId = ToUInt16(data.Slice(5));
                            AttachedPeripherals.Update(channel, info => info.WithDevice(deviceId));
                        }
                        else if (eventType == HUB_EVENT_DETACHED)
                        {
                            AttachedPeripherals.Remove(channel);
                        }
                    }
                    return true;
                }
#if DEBUG
            case 0x02: // Hub actions
                Dump("Hub actions", data);
                break;

            case 0x03: // Hub alerts
                Dump("Hub alerts", data);
                break;

            case 0x05: // Generic error messages
                Dump("Generic error messages", data);
                break;

            case 0x08: // HW network commands
                Dump("HW network commands", data);
                break;

            case 0x13: // FW lock status
                Dump("FW lock status", data);
                break;

            case 0x43: // Port information
                Dump("Port information", data);
                break;

            case 0x44: // Port mode information
                Dump("Port mode information", data);
                break;

            case 0x47: // Port input format (Single mode)
                Dump("Port input format (single)", data);
                break;

            case 0x48: // Port input format (Combined mode)
                Dump("Port input format (combined)", data);
                break;

            case MESSAGE_TYPE_OUTPUT_COMMAND_FEEDBACK: // Port output command feedback
                Dump("Output command feedback", data);
                break;
#endif
        }
        return false;
    }

    protected async ValueTask<bool> WriteNoResponseAsync(byte[] data, TimeSpan sentDelay, CancellationToken token = default)
    {
        var result = await _bleDevice!.WriteNoResponseAsync(Characteristic!, data, token);
        await Task.Delay(sentDelay, token);
        return result;
    }

    protected async ValueTask<bool> WriteNoResponseAsync(byte[] data, CancellationToken token = default)
        => await _bleDevice!.WriteNoResponseAsync(Characteristic!, data, token);

    protected async ValueTask<bool> WriteAsync(byte[] data, CancellationToken token = default)
        => await _bleDevice!.WriteAsync(Characteristic!, data, token);

    protected static Task DelayAsync(CancellationToken token = default) => Task.Delay(20, token);

    protected ChannelOutputType GetOutputType(int channel) => ChannelConfigs.Get(channel).OutputType;

    protected int GetMaxServoAngle(int channel)
    {
        var maxServoAngle = ChannelConfigs.Get(channel).MaxServoAngle;
        return maxServoAngle > 0 ? maxServoAngle : DEFAULT_MAX_SERVO_ANGLE;
    }

    protected int GetAbsPosition(int channel) => ChannelAbsPositions.Get(channel).Current;

    protected async ValueTask RequestHubPropertiesAsync(CancellationToken token)
    {
        try
        {
            // Request firmware version
            await RequestHubPropertyAsync(HUB_PROPERTY_FW_VERSION, token);
            // Request hardware version
            await RequestHubPropertyAsync(HUB_PROPERTY_HW_VERSION, token);
            // Request battery voltage
            await RequestHubPropertyAsync(HUB_PROPERTY_VOLTAGE, token);
        }
        catch { }
    }

    protected async ValueTask RequestHubPropertyAsync(byte propertyId, CancellationToken token)
    {
        try
        {
            // Request firmware version
            await Task.Delay(TimeSpan.FromMilliseconds(100), token);
            await _bleDevice!.WriteAsync(Characteristic!, [0x05, 0x00, 0x01, propertyId, 0x05], token);
            var data = await _bleDevice!.ReadAsync(Characteristic!, token);
            ProcessHubPropertyData(data);
        }
        catch { }
    }

    protected void ProcessHubPropertyData(ReadOnlySpan<byte> data)
    {
        try
        {
            if (data.Length < 6)
            {
                return;
            }

            var messageId = data[2];
            var propertyId = data[3];
            var propertyOperation = data[4];

            if (messageId != MESSAGE_TYPE_HUB_PROPERTIES || propertyOperation != HUB_PROPERTY_OPERATION_UPDATE)
            {
                // Operation is not 'update'
                return;
            }

            switch (propertyId)
            {
                case HUB_PROPERTY_FW_VERSION: // FW version
                    var firmwareVersion = GetVersionString(data.Slice(5));
                    if (!string.IsNullOrEmpty(firmwareVersion))
                    {
                        FirmwareVersion = firmwareVersion;
                    }
                    break;

                case HUB_PROPERTY_HW_VERSION: // HW version
                    var hardwareVersion = GetVersionString(data.Slice(5));
                    if (!string.IsNullOrEmpty(hardwareVersion))
                    {
                        HardwareVersion = hardwareVersion;
                    }
                    break;

                case HUB_PROPERTY_VOLTAGE: // Battery voltage
                    var voltage = data[5];
                    BatteryVoltage = voltage.ToString("F0");
                    break;
            }
        }
        catch { }
    }

    protected static ValueTask<bool> AwaitStablePositionAsync(Func<PositionInfo> getValue, TimeSpan timeout, CancellationToken token)
       => WaitForStableValueAsync(getValue,
            stabilityCheck: (value, last) =>
                (value.IsUpdated && value.Current == last.Current) ||
                (!value.IsUpdated && value.UpdateTime == last.UpdateTime),
            timeout,
            token);

    protected static ValueTask<bool> AwaitPositionChangeAsync(Func<PositionInfo> getValue, TimeSpan timeout, CancellationToken token)
        => WaitForStableValueAsync(getValue: getValue,
            stabilityCheck: (value, last) => value.IsUpdated,
            timeout,
            stabilityTimeout: TimeSpan.FromMilliseconds(50),
            token);

    protected async ValueTask<bool> AwaitPeripheralsAttachedAsync(TimeSpan timeout, CancellationToken token)
    {
        var result = await WaitForStableValueAsync(
            getValue: () => AttachedPeripherals.Max(x => x.UpdateTime),
            stabilityCheck: (value, last) => AttachedPeripherals.Count > 0 &&
                 value != DateTime.MinValue &&
                 last == value,
            timeout,
            token);

       return result && AttachedPeripherals.Count > 0;
    }
}
