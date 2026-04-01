using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


using static BrickController2.Protocols.LegoWirelessProtocol;

namespace BrickController2.DeviceManagement.Lego;

internal abstract class ControlPlusDeviceBase : BluetoothDevice
{
    protected IGattCharacteristic? Characteristic;

    protected ControlPlusDeviceBase(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
     : base(name, address, deviceRepository, bleService)
    {
    }

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
}
