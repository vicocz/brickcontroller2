using System;
using System.Text;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement;

/// <summary>
/// Manager for PfxBrick devices
/// </summary>
public class PfxBrickDeviceManager : IBluetoothLEDeviceManager
{
    // hardcoded name to detects PFX Brick devices
    private static readonly byte[] CompleteLocalName = Encoding.ASCII.GetBytes("PFx Brick 16 MB");

    public bool TryGetDevice(ScanResult scanResult, out FoundDevice device)
    {
        // check if there are any data and it matches complete local name: 
        if (scanResult.TryGetCompleteLocalName(out var localName) && localName.SequenceEqual(CompleteLocalName))
        {
            device = new FoundDevice(DeviceType.PfxBrick, scanResult.DeviceName, scanResult.DeviceAddress);
            return true;
        }

        device = default;
        return false;
    }
}
