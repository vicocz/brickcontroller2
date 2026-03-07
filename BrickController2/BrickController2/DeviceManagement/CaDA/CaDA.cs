using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.Vendors;
using BrickController2.Extensions;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement.CaDA;

/// <summary>
/// Vendor: CaDa with all its devices and implementation of IBluetoothLEDeviceManager
/// </summary>
internal class CaDa : Vendor<CaDa>
{
    public override string VendorName => "CaDA";

    protected override void Register(VendorBuilder<CaDa> builder)
    {
        // classic devices
        builder.ContainerBuilder.RegisterDevice<CaDARaceCar>(DeviceType.CaDA_RaceCar);

        // device manager
        builder.RegisterDeviceManager<CaDADeviceManager>()
            .As<IBluetoothLEAdvertiserDeviceScanInfo>()
            .As<ICaDADeviceManager>()
            .SingleInstance();
    }
}
