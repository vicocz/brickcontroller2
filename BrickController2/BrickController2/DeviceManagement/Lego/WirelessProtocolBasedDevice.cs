using BrickController2.CreationManagement;
using BrickController2.DeviceManagement.IO;
using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static BrickController2.CreationManagement.ControllerDefaults;
using static BrickController2.Diagnostics.Logs;
using static BrickController2.Protocols.LegoWirelessProtocol;

namespace BrickController2.DeviceManagement.Lego;

internal abstract class WirelessProtocolBasedDevice : BluetoothDevice
{
    protected readonly ChannelStateStore<ChannelConfig> ChannelConfigs;
    protected readonly ChannelStateStore<ChannelPositionState> ChannelAbsPositions;
    protected readonly ChannelStateStore<ChannelPositionState> ChannelRelativePositions;

    protected readonly ChannelStateStore<ChannelAttachmentInfo> AttachedPeripherals;

    protected IGattCharacteristic? Characteristic;

    protected WirelessProtocolBasedDevice(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
     : base(name, address, deviceRepository, bleService)
    {
        ChannelConfigs = new();
        ChannelAbsPositions = new(ChannelPositionState.Initial);
        ChannelRelativePositions = new(ChannelPositionState.Initial);
        AttachedPeripherals = new(ChannelAttachmentInfo.Initial);
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
        ChannelAbsPositions.Clear();
        ChannelRelativePositions.Clear();
        AttachedPeripherals.Clear();

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

            ChannelConfigs.Set(i, config.ChannelOutputType switch
            {
                ChannelOutputType.ServoMotor => new()
                {
                    OutputType = ChannelOutputType.ServoMotor,
                    MaxServoAngle = config.MaxServoAngle,
                    ServoBaseAngle = config.ServoBaseAngle
                },
                ChannelOutputType.StepperMotor => new()
                {
                    OutputType = ChannelOutputType.StepperMotor,
                    StepperAngle = config.StepperAngle
                },
                _ => new()
            });
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
        // reset output values & positions
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

    protected virtual Task<bool> SendOutputValuesAsync(CancellationToken token) => throw new InvalidOperationException(nameof(SendOutputValuesAsync));

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
        if (data.Length < 4 || Characteristic is null || characteristicGuid != Characteristic.Uuid)
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
                    if (!TryGetChannelIndex(portId: data[3], out var channel))
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
                        // TODO: Read the post value format response and determine the value length accordingly
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
                        byte eventType = data[4]; // 0x01 = Attached, 0x00 = Detached, 0x02 = Attached Virtual

                        if (eventType == 0x01 || eventType == 0x02)
                        {
                            var deviceId = ToUInt16(data.Slice(5));
                            AttachedPeripherals.Update(channel, info => info.WithDevice(deviceId)); // store portId as "position" for simplicity
                        }
                        else if (eventType == 0x00)
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

            var dataLength = data[0];
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

    protected int CalculateCalibratedTarget(int channel, int targetBaseAngle = 0)
    {
        var position = ChannelRelativePositions.Get(channel).Current;

        // Normalize the hardware relative angle to a clean 0-359 range 
        // (Crucial if your motor firmware reports APOS as -180 to 179)
        int normalizedRelative = ((position % 360) + 360) % 360;
        int normalizedTarget = ((targetBaseAngle % 360) + 360) % 360;

        // Calculate the raw difference + normalize
        int diff = NormalizeAngle(normalizedTarget - normalizedRelative);

        // Offset the current accumulated position by the physical difference
        return GetAbsPosition(channel) + diff;
    }

    protected Task AwaitStableRelativePositionAsync(int channel, TimeSpan timeout, CancellationToken token)
        => WaitForStablePositionAsync(timeout, () => ChannelRelativePositions.Get(channel), token);

    protected Task AwaitStableAbsolutePositionAsync(int channel, TimeSpan timeout, CancellationToken token)
        => WaitForStablePositionAsync(timeout, () => ChannelAbsPositions.Get(channel), token);

    protected Task AwaitForPeripheralsAttachedAsync(TimeSpan timeout, CancellationToken token)
        => WaitForTimestampStabilityAsync(timeout, () => AttachedPeripherals.Max(x => x.UpdateTime), token);

    private static async Task WaitForTimestampStabilityAsync(TimeSpan timeout, Func<DateTime> getTimestamp, CancellationToken token)
    {
        var interval = TimeSpan.FromMilliseconds(50);
        var stabilityTimeout = TimeSpan.FromMilliseconds(200);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        linkedCts.CancelAfter(timeout);

        var stableSince = Stopwatch.StartNew();
        var lastUpdated = getTimestamp();

        try
        {
            while (!linkedCts.Token.IsCancellationRequested)
            {
                await Task.Delay(interval, linkedCts.Token);

                var currentValue = getTimestamp();
                if (currentValue != lastUpdated)
                {
                    lastUpdated = currentValue;
                    stableSince.Restart();
                }
                else if (stableSince.Elapsed >= stabilityTimeout)
                {
                    return; // position stable for the required duration
                }
            }
            Dump("TimestampStability: TIMEOUT", lastUpdated);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            // total timeout elapsed — treat as completed
            Dump("TimestampStability: CANCELLED", lastUpdated);
        }
    }

    private static async Task WaitForStablePositionAsync(TimeSpan timeout, Func<ChannelPositionState> getPosition, CancellationToken token)
    {
        var interval = TimeSpan.FromMilliseconds(50);
        var stabilityTimeout = TimeSpan.FromMilliseconds(500);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        linkedCts.CancelAfter(timeout);

        var lastPosition = getPosition().Current;
        var stableSince = Stopwatch.StartNew();

        try
        {
            while (!linkedCts.Token.IsCancellationRequested)
            {
                await Task.Delay(interval, linkedCts.Token);

                var currentPosition = getPosition();
                if (!currentPosition.IsUpdated || currentPosition.Current != lastPosition)
                {
                    lastPosition = currentPosition.Current;
                    stableSince.Restart();
                }
                else if (stableSince.Elapsed >= stabilityTimeout)
                {
                    Dump("Servo position: STABLE", lastPosition);
                    return; // position stable for the required duration
                }
            }
            Dump("Servo position: TIMEOUT", lastPosition);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            // total timeout elapsed — treat as completed
            Dump("Servo position: CANCELLED", lastPosition);
        }
    }

}
