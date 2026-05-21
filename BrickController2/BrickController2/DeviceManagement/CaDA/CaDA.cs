using Autofac;
using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.Vendors;
using BrickController2.Extensions;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement.CaDA;

/// <summary>
/// Vendor: CaDA with all its devices and implementation of IBluetoothLEDeviceManager
/// </summary>
internal class CaDA : Vendor<CaDA>
{
    public override string VendorName => "CaDA";

    protected override void Register(VendorBuilder<CaDA> builder)
    {
        // classic devices
        builder.ContainerBuilder.RegisterDevice<CaDARaceCar>(DeviceType.CaDA_RaceCar);

        // device manager
        builder.RegisterDeviceManager<CaDADeviceManager>()
            .As<IBluetoothLEAdvertiserDeviceScanInfo>()
            .As<ICaDADeviceManager>()
            .SingleInstance();

        // additional dependencies
        builder.ContainerBuilder.RegisterType<MessageEncoderFactory>()
            .As<IMessageEncoderFactory>()
            .SingleInstance();
    }
}
