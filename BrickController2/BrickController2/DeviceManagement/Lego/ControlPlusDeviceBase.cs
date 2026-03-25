using BrickController2.DeviceManagement.IO;
using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static BrickController2.Protocols.LegoWirelessProtocol;

namespace BrickController2.DeviceManagement.Lego;

internal abstract class ControlPlusDeviceBase : BluetoothDevice
{
    protected static readonly TimeSpan SEND_DELAY = TimeSpan.FromMilliseconds(25);

    private DeviceInitializationWaiter? _initializationWaiter;

    protected readonly HashSet<int> AttachedChannels = [];

    protected IGattCharacteristic? Characteristic;

    protected ControlPlusDeviceBase(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
        : base(name, address, deviceRepository, bleService)
    {
    }

    public override string BatteryVoltageSign => "%";

    protected virtual byte GetPortId(int channelIndex) => (byte)channelIndex;
    protected virtual bool TryGetChannelIndex(byte portId, out int channelIndex)
    {
        channelIndex = portId;
        return portId < NumberOfChannels;
    }

    protected override async Task<bool> ValidateServicesAsync(IEnumerable<IGattService>? services, CancellationToken token)
    {
        var service = services?.FirstOrDefault(s => s.Uuid == ServiceUuid);
        Characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CharacteristicUuid);

        if (Characteristic is not null)
        {
            // reset channel state
            AttachedChannels.Clear();
            // init waiter
            _initializationWaiter = new();

            return await _bleDevice!.EnableNotificationAsync(Characteristic, token);
        }

        return false;
    }

    protected override void OnDeviceDisconnecting()
    {
        // reset channel state
        AttachedChannels.Clear();
        _initializationWaiter = null;
        Characteristic = null;
    }

    protected override void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
    {
        if (characteristicGuid != CharacteristicUuid || data.Length < 4)
        {
            return;
        }

        var messageCode = data[2];

        switch (messageCode)
        {
            case 0x01: // Hub properties
                ProcessHubPropertyData(data);
                break;

            case 0x02: // Hub actions
                DumpData("Hub actions", data);
                break;

            case 0x03: // Hub alerts
                DumpData("Hub alerts", data);
                break;

            case 0x04: // Hub attached I/O
                DumpData("Hub attached I/O", data);
                byte portId = data[3];
                byte eventType = data[4]; // 0x01 = Attached, 0x00 = Detached, 0x02 = Attached Virtual

                if (TryGetChannelIndex(portId, out var channel))
                {
                    if (eventType == 0x01 || eventType == 0x02)
                    {
                        AttachedChannels.Add(channel);
                        _initializationWaiter?.NotifyPortAttached();
                    }
                    else if (eventType == 0x00)
                    {
                        AttachedChannels.Remove(channel);
                    }
                }
                break;

            case 0x05: // Generic error messages
                DumpData("Generic error messages", data);
                break;

            case 0x08: // HW network commands
                DumpData("HW network commands", data);
                break;

            case 0x13: // FW lock status
                DumpData("FW lock status", data);
                break;

            case 0x43: // Port information
                DumpData("Port information", data);
                break;

            case 0x44: // Port mode information
                DumpData("Port mode information", data);
                break;

            case 0x45: // Port value (single mode)
                //lock (_positionLock)
                //{
                //    if (data.Length == 6)
                //    {
                //        // assume 16bit data is ABS
                //        if (TryGetChannelIndex(data[3], out var channel))
                //        {
                //            var absPosition = ToInt16(data, 4);
                //            _absolutePositions[channel] = absPosition;
                //        }
                //    }
                //    else if (data.Length == 8)
                //    {
                //        // assume 32 bit data is REL
                //        if (TryGetChannelIndex(data[3], out var channel))
                //        {
                //            var relPosition = ToInt32(data, 4);
                //            _relativePositions[channel] = relPosition;

                //            _positionsUpdated[channel] = true;
                //            _positionUpdateTimes[channel] = DateTime.Now;
                //        }
                //    }
                //}
                break;

            case 0x46: // Port value (combined mode)
                //lock (_positionLock)
                //{
                //    if (!TryGetChannelIndex(data[3], out var channel))
                //    {
                //        break;
                //    }

                //    var modeMask = data[5];
                //    var dataIndex = 6;

                //    if ((modeMask & 0x01) != 0)
                //    {
                //        var absPosition = ToInt32(data, dataIndex);
                //        _absolutePositions[channel] = absPosition;

                //        dataIndex += 2;
                //    }

                //    if ((modeMask & 0x02) != 0)
                //    {
                //        // TODO: Read the post value format response and determine the value length accordingly
                //        if ((dataIndex + 3) < data.Length)
                //        {
                //            var relPosition = ToInt32(data, dataIndex);
                //            _relativePositions[channel] = relPosition;
                //        }
                //        else if ((dataIndex + 1) < data.Length)
                //        {
                //            var relPosition = ToInt16(data, dataIndex);
                //            _relativePositions[channel] = relPosition;
                //        }
                //        else
                //        {
                //            _relativePositions[channel] = data[dataIndex];
                //        }

                //        _positionsUpdated[channel] = true;
                //        _positionUpdateTimes[channel] = DateTime.Now;
                //    }
                //}

                break;

            case 0x47: // Port input format (Single mode)
                DumpData("Port input format (single)", data);
                break;

            case 0x48: // Port input format (Combined mode)
                DumpData("Port input format (combined)", data);
                break;

            case 0x82: // Port output command feedback
                OnPortOutputCommandFeedback(data);
                break;
        }
    }

    protected virtual void OnPortOutputCommandFeedback(ReadOnlySpan<byte> data)
    {
#if DEBUG
        DumpData("Output command feedback", data);
#endif
    }

    protected virtual void OnHubPropertyData(byte messageId, byte propertyId, ReadOnlySpan<byte> propertyData)
    {
        switch (propertyId)
        {
            case HUB_PROPERTY_FW_VERSION: // FW version
                var firmwareVersion = GetVersionString(propertyData);
                if (!string.IsNullOrEmpty(firmwareVersion))
                {
                    FirmwareVersion = firmwareVersion;
                }
                break;

            case HUB_PROPERTY_HW_VERSION: // HW version
                var hardwareVersion = GetVersionString(propertyData);
                if (!string.IsNullOrEmpty(hardwareVersion))
                {
                    HardwareVersion = hardwareVersion;
                }
                break;

            case HUB_PROPERTY_VOLTAGE: // Battery voltage
                var voltage = propertyData[0];
                BatteryVoltage = voltage.ToString("F0");
                break;
        }
    }

    protected async Task<bool> WriteNoResponseAsync(byte[] data, TimeSpan sentDelay, CancellationToken token = default)
    {
        var result = await _bleDevice!.WriteNoResponseAsync(Characteristic!, data, token);
        await Task.Delay(sentDelay, token);
        return result;
    }

    protected Task<bool> WriteNoResponseAsync(byte[] data, CancellationToken token = default)
        => _bleDevice!.WriteNoResponseAsync(Characteristic!, data, token);

    protected Task<bool> WriteAsync(byte[] data, CancellationToken token = default)
        => _bleDevice!.WriteAsync(Characteristic!, data, token);

    protected async Task RequestHubPropertiesAsync(CancellationToken token)
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

    protected async Task RequestHubPropertyAsync(byte propertyId, CancellationToken token)
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

    /// <summary>
    /// Call this immediately after enabling characteristic notifications.
    /// </summary>
    protected Task<bool> WaitForInitializationAsync(CancellationToken token)
        => _initializationWaiter?.WaitAsync(token) ?? Task.FromResult(false);

    private void ProcessHubPropertyData(ReadOnlySpan<byte> data)
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

            OnHubPropertyData(messageId, propertyId, data[5..]);
        }
        catch { }
    }


    private static void DumpData(string header, ReadOnlySpan<byte> data)
    {
#if DEBUG
        var s = Convert.ToHexString(data);
        Debug.WriteLine(DateTime.Now + " " + header + " - " + s);
#endif
    }
}
