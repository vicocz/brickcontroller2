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
internal class RemoteControl : WirelessProtocolBasedDevice, IDeviceType<RemoteControl>, IDynamicInputDevice
{
    private const string ENABLED_SETTING_NAME = "RemoteControlEnabled";
    private const bool DEFAULT_ENABLED = false;

    private static readonly IReadOnlyCollection<string> AllButtons =
    [
        "A", "B", "Home",
        "A.Minus", "A.Plus",
        "B.Minus", "B.Plus"
    ];

    private IInputDeviceConnector? _inputDeviceConnector;

    public RemoteControl(string name, string address, IEnumerable<NamedSetting> settings, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
    : base(name, address, deviceRepository, bleService)
    {
        SetSettingValue(ENABLED_SETTING_NAME, settings, DEFAULT_ENABLED);
    }

    public static string TypeName => "Remote Control";
    public static DeviceType Type => DeviceType.RemoteControl;

    public override DeviceType DeviceType => DeviceType.RemoteControl;

    public override int NumberOfChannels => 0;

    public bool IsEnabled => GetSettingValue(ENABLED_SETTING_NAME, DEFAULT_ENABLED);

    public override void SetOutput(int channel, float value) => throw new InvalidOperationException();

    public void ConnectInputController(IInputDeviceConnector inputController)
    {
        _inputDeviceConnector = inputController;
    }

    public void DisconnectInputController()
    {
        _inputDeviceConnector = default;
    }

    internal void ResetEvents() => RaiseButtonEvents(BUTTON_RELEASED);

    protected override Task ProcessOutputsAsync(CancellationToken token) => Task.CompletedTask;

    protected override Task<bool> SendOutputValuesAsync(CancellationToken token) => throw new InvalidOperationException();

    protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
    {
        // wait until ports finish communicating with the hub
        await AwaitPeripheralsAttachedAsync(TimeSpan.FromMilliseconds(250), token);

        if (requestDeviceInformation)
        {
            await RequestHubPropertiesAsync(token);
        }

        // setup ports - 0x04 - REMOTE_MODE_KEYS
        var remoteButtonA = BuildPortInputFormatSetup(REMOTE_BUTTONS_LEFT, REMOTE_MODE_KEYS, interval: 1);
        await WriteAsync(remoteButtonA, token);

        var remoteButtonB = BuildPortInputFormatSetup(REMOTE_BUTTONS_RIGHT, REMOTE_MODE_KEYS, interval: 1);
        return await WriteAsync(remoteButtonB, token);
    }

    protected override bool TryProcessMessageData(byte messageType, ReadOnlySpan<byte> data)
    {
        switch (messageType)
        {
            case MESSAGE_TYPE_HW_NETWORK_COMMANDS: // HW network commands
                if (data.Length == 5 && data[3] == 0x02)
                {
                    // HW button state
                    _inputDeviceConnector.RaiseButtonEventConditionally("Home", IsButtonPressed(data[4]));
                    return true;
                }
                break;
            case MESSAGE_TYPE_PORT_VALUE: // 0x45 Port Value / RemoteButton
                if (data.Length == 7)
                {
                    switch (data[3])
                    {
                        case REMOTE_BUTTONS_LEFT:
                            OnButtonEvents("A.Plus", "A", "A.Minus", data.Slice(4));
                            break;
                        case REMOTE_BUTTONS_RIGHT:
                            OnButtonEvents("B.Plus", "B", "B.Minus", data.Slice(4));
                            break;
                    }
                    return true;
                }
                break;
        }
        return base.TryProcessMessageData(messageType, data);
    }

    private void OnButtonEvents(string plus, string stop, string minus, ReadOnlySpan<byte> flags)
    {
        _inputDeviceConnector.RaiseButtonEventConditionally(plus, IsButtonPressed(flags[0]));
        _inputDeviceConnector.RaiseButtonEventConditionally(stop, IsButtonPressed(flags[1]));
        _inputDeviceConnector.RaiseButtonEventConditionally(minus, IsButtonPressed(flags[2]));
    }

    private void RaiseButtonEvents(float value)
    {
        if (_inputDeviceConnector is null)
        {
            return;
        }

        var events = AllButtons
            .Where(e => _inputDeviceConnector.HasValueChanged(InputDeviceEventType.Button, e, value))
            .ToDictionary(e => (InputDeviceEventType.Button, e), e => value);

        _inputDeviceConnector.RaiseEvent(events);
    }

    private static bool IsButtonPressed(byte flag) => flag != 0;
}
