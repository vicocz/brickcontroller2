using BrickController2.InputDeviceManagement;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.PlatformServices.InputDevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    public void Start()
    {
        //TODO
        _ = base.ConnectAsync(false, (d) => { }, [], false, false, default);
    }

    public void Stop()
    {
        _ = base.DisconnectAsync();
    }

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
         // setup ports - 0x03 SI The SI value range
         var remoteButtonA = BuildPortInputFormatSetup(0, PORT_MODE_3);
         await _bleDevice!.WriteAsync(_characteristic!, remoteButtonA, token);

         var remoteButtonB = BuildPortInputFormatSetup(1, PORT_MODE_3);
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
                    OnButtonEvent("Green", data[4] & 0x01);
                    break;
                }
                break;
            case 0x45: // 0x45	RemoteButton
                if (data.Length == 5)
                {
                    // port buttons
                    string port = data[3] switch
                    {
                        0 => "ButtonA",
                        1 => "ButtonB",
                        _ => "Unknown"
                    };
                    OnButtonEvent(port + "+", data[4] & 0x01); // + button
                    OnButtonEvent(port + "Red", data[4] & 0x02); // Red button
                    OnButtonEvent(port + "-", data[4] & 0x04); // - button
                    break;
                }
                break;
            default:
                break;
        }
    }

    private void OnButtonEvent(string button, int flag) => _legoController?.OnButtonEvent(button, flag > 0 ? 1.0f : 0.0f);
}
