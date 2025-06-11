using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BrickController2.Protocols.LegoWirelessProtocol;

namespace BrickController2.DeviceManagement.Lego;

internal class LegoController : ControlPlusDevice
{
    public LegoController(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
    : base(name, address, deviceRepository, bleService)
    {
    }

    public override DeviceType DeviceType => DeviceType.LegoController;

    public override int NumberOfChannels => 0;

    protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
    {
        if (await base.AfterConnectSetupAsync(requestDeviceInformation, token))
        {
            // setup ports - 0x03 SI The SI value range
            var remoteButtonA = BuildPortInputFormatSetup(0, PORT_MODE_3);
            await WriteAsync(remoteButtonA, token);

            var remoteButtonB = BuildPortInputFormatSetup(1, PORT_MODE_3);
            return await WriteAsync(remoteButtonB, token);
        }

        return false;
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
                    bool  pressed = data[4] == 0x01;
                    break;
                }
                break;
            case 0x45: // 0x45	RemoteButton
                if (data.Length == 5)
                {
                    // HW button state
                    byte portId = data[3]; // port id
                    bool pressed = data[4] == 0x01;
                    break;
                }
                break;
            default:
                base.OnCharacteristicChanged(characteristicGuid, data);
                break;
        }
    }
}
