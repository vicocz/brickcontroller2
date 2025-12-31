using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.PlatformServices.InputDevice;
using BrickController2.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static BrickController2.PlatformServices.InputDevice.InputDevices;
using static BrickController2.Protocols.LegoWirelessProtocol;

namespace BrickController2.DeviceManagement.Lego;

/// <summary>
/// Represents a LEGO® Powered Up 88010 Remote Control
/// </summary>
internal class RemoteControl : BluetoothDevice
{
    private const string ENABLED_SETTING_NAME = "RemoteControlEnabled";
    private const bool DEFAULT_ENABLED = false;

    private IGattCharacteristic? _characteristic;
    private InputDeviceBase<RemoteControl>? _inputController;

    public RemoteControl(string name, string address, IEnumerable<NamedSetting> settings, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
    : base(name, address, deviceRepository, bleService)
    {
        SetSettingValue(ENABLED_SETTING_NAME, settings, DEFAULT_ENABLED);
    }

    public override DeviceType DeviceType => DeviceType.RemoteControl;

    public override int NumberOfChannels => 0;

    public override string BatteryVoltageSign => "%";

    public bool IsEnabled => GetSettingValue(ENABLED_SETTING_NAME, DEFAULT_ENABLED);

    protected override bool AutoConnectOnFirstConnect => false;

    public override void SetOutput(int channel, float value) => throw new InvalidOperationException();

    internal void ConnectInputController<TController>(TController inputController) where TController : InputDeviceBase<RemoteControl>
    {
        _inputController = inputController;
    }

    internal void DisconnectInputController()
    {
        _inputController = default;
    }

    internal void ResetEvents() => RaiseButtonEvents(
    [
        ("A", BUTTON_RELEASED),
        ("B", BUTTON_RELEASED),
        ("Home", BUTTON_RELEASED),
        ("A.Minus", BUTTON_RELEASED),
        ("A.Plus", BUTTON_RELEASED),
        ("B.Minus", BUTTON_RELEASED),
        ("B.Plus", BUTTON_RELEASED)
    ]);

    protected override Task ProcessOutputsAsync(CancellationToken token) => Task.CompletedTask;

    protected override async Task<bool> ValidateServicesAsync(IEnumerable<IGattService>? services, CancellationToken token)
    {
        var service = services?.FirstOrDefault(s => s.Uuid == ServiceUuid);
        _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CharacteristicUuid);

        if (_characteristic is not null)
        {
            return await _bleDevice!.EnableNotificationAsync(_characteristic, token);
        }

        return false;
    }

    protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
    {
        // wait until ports finish communicating with the hub
        await Task.Delay(250, token);

        if (requestDeviceInformation)
        {
            // Request battery voltage
            await _bleDevice!.WriteAsync(_characteristic!, [0x05, 0x00, 0x01, 0x06, 0x05], token);
            await Task.Delay(TimeSpan.FromMilliseconds(50), token);
        }

        // setup ports - 0x04 - REMOTE_MODE_KEYS
        var remoteButtonA = BuildPortInputFormatSetup(REMOTE_BUTTONS_LEFT, REMOTE_MODE_KEYS, interval: 1);
        await _bleDevice!.WriteAsync(_characteristic!, remoteButtonA, token);

        var remoteButtonB = BuildPortInputFormatSetup(REMOTE_BUTTONS_RIGHT, REMOTE_MODE_KEYS, interval: 1);
        return await _bleDevice!.WriteAsync(_characteristic!, remoteButtonB, token);
    }

    protected override void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
    {
        if (data.Length < 4)
        {
            return;
        }

        var messageCode = data[2];

        switch (messageCode)
        {
            case MESSAGE_TYPE_HUB_PROPERTIES: // Hub properties
                if (data.Length >= 6 &&
                    data[3] == HUB_PROPERTY_VOLTAGE &&
                    data[4] == HUB_PROPERTY_OPERATION_UPDATE)
                {
                    BatteryVoltage = data[5].ToString("F0");
                }
                break;

            case MESSAGE_TYPE_HW_NETWORK_COMMANDS: // HW network commands
                if (data.Length == 5 && data[3] == 0x02)
                {
                    // HW button state
                    RaiseButtonEvents([("Home", GetButtonValue(data[4]))]);
                    break;
                }
                break;
            case MESSAGE_TYPE_PORT_VALUE: // 0x45 Port Value / RemoteButton
                if (data.Length == 7)
                {
                    switch (data[3])
                    {
                        case REMOTE_BUTTONS_LEFT:
                            OnButtonEvents("A.Plus", "A", "A.Minus", data.AsSpan(4));
                            break;
                        case REMOTE_BUTTONS_RIGHT:
                            OnButtonEvents("B.Plus", "B", "B.Minus", data.AsSpan(4));
                            break;
                        default:
                            break;
                    }
                }
                break;
            default:
                break;
        }
    }

    private void OnButtonEvents(string plus, string stop, string minus, ReadOnlySpan<byte> flags)
        => RaiseButtonEvents(
            [
                (plus, GetButtonValue(flags[0])),
                (stop, GetButtonValue(flags[1])),
                (minus, GetButtonValue(flags[2]))
            ]);

    private void RaiseButtonEvents((string eventName, float value)[] buttonEvents)
    {
        if (_inputController is null)
        {
            return;
        }

        var events = buttonEvents
            .Where(e => _inputController.HasValueChanged(e.eventName, e.value))
            .ToDictionary(e => (InputDeviceEventType.Button, e.eventName), e => e.value);

        _inputController.RaiseEvent(events);
    }

    private static float GetButtonValue(byte flag) => flag != 0 ? BUTTON_PRESSED : BUTTON_RELEASED;
}
