using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.Vendors;
using BrickController2.Extensions;

namespace BrickController2.DeviceManagement.Vengit;

/// <summary>
/// Vendor: Vengit and all its device types: SBrick, SBrick Plus, SBrick Light and implementation of IBluetoothLEDeviceManager
/// </summary>
internal class Vengit : Vendor<Vengit>
{
    public override string VendorName => "Vengit";

    protected override void Register(VendorBuilder<Vengit> builder)
    {
        // classic devices
        builder.ContainerBuilder
            .RegisterDevice<SBrickDevice>(DeviceType.SBrick)
            .RegisterDevice<SBrickLightDevice>(DeviceType.SBrickLight);

        // device manager
        builder.RegisterDeviceManager<SBrickDeviceManager>();
    }
}
