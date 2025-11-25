using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.PlatformServices.InputDevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static BrickController2.PlatformServices.InputDevice.InputDevices;
using static BrickController2.Protocols.LegoWirelessProtocol;

namespace BrickController2.DeviceManagement.Lego;

internal class LegoRemoteControl : BluetoothDevice
{
    private static readonly Guid SERVICE_UUID = new("00001623-1212-efde-1623-785feabcd123");
    private static readonly Guid CHARACTERISTIC_UUID = new("00001624-1212-efde-1623-785feabcd123");

    private IGattCharacteristic? _characteristic;
    private LegoController? _legoController;

    public LegoRemoteControl(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
    : base(name, address, deviceRepository, bleService)
    {
    }

    public override DeviceType DeviceType => DeviceType.LegoRemoteControl;

    public override int NumberOfChannels => 0;

    protected override bool AutoConnectOnFirstConnect => false;

    public override void SetOutput(int channel, float value) => throw new InvalidOperationException();

    internal void LinkLegoController(LegoController? legoController)
    {
        _legoController = legoController;
    }

    protected override Task ProcessOutputsAsync(CancellationToken token) => throw new InvalidOperationException();

    protected override async Task<bool> ValidateServicesAsync(IEnumerable<IGattService>? services, CancellationToken token)
    {
        var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
        _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID);

        if (_characteristic is not null)
        {
            return await _bleDevice!.EnableNotificationAsync(_characteristic, token);
        }

        return false;
    }

    protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
    {
        await Task.Delay(250, token);

        // setup ports - 0x04 - REMOTE_BUTTONS_MODE_KEYSD
        var remoteButtonA = BuildPortInputFormatSetup(REMOTE_BUTTONS_LEFT, REMOTE_MODE_KEYSD, interval: 1);
        await _bleDevice!.WriteAsync(_characteristic!, remoteButtonA, token);

        var remoteButtonB = BuildPortInputFormatSetup(REMOTE_BUTTONS_RIGHT, REMOTE_MODE_KEYSD, interval: 1);
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
            case 0x08: // HW network commands
                if (data.Length == 5 && data[3] == 0x02)
                {
                    // HW button state
                    _legoController?.RaiseButtonEvent("Home", data[4] > 0);
                    break;
                }
                break;
            case 0x45: // 0x45	RemoteButton
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
        => _legoController?.RaiseEvents(new()
            {
                { (InputDeviceEventType.Button, plus), flags[0] == 0 ? BUTTON_RELEASED : BUTTON_PRESSED },
                { (InputDeviceEventType.Button, stop), flags[1] == 0 ? BUTTON_RELEASED : BUTTON_PRESSED },
                { (InputDeviceEventType.Button, minus), flags[2] == 0 ? BUTTON_RELEASED : BUTTON_PRESSED }
            });
}
