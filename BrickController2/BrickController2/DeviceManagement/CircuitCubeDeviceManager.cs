using System;

namespace BrickController2.DeviceManagement;

/// <summary>
/// Manager for CircuitCube devices
/// </summary>
public class CircuitCubeDeviceManager : BluetoothDeviceManagerBase
{
    protected override bool TryGetDeviceByServiceUiid(FoundDevice template, Guid serviceGuid, out FoundDevice device)
    {
        if (serviceGuid == CircuitCubeDevice.SERVICE_UUID && !string.IsNullOrEmpty(template.DeviceName))
        {
            device = template with { DeviceType = DeviceType.CircuitCubes };
            return true;
        }
        // no match
        device = default;
        return false;
    }
}
