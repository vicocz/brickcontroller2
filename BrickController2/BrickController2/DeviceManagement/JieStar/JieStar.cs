using Autofac;
using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.Vendors;

namespace BrickController2.DeviceManagement.JieStar;

/// <summary>
/// Vendor: JieStar and all its devices
/// </summary>
internal class JieStar : Vendor<JieStar>
{
    public override string VendorName => "Jie Star";

    protected override void Register(VendorBuilder<JieStar> builder)
    {
        // device manager
        builder.ContainerBuilder.RegisterType<JieStarDeviceManager>()
            .As<IJieStarDeviceManager>()
            .SingleInstance();

        // manually added devices
        builder.RegisterDevice<JieStarSCM4>()
            .WithDeviceFactory(JieStarSCM4.Device, $"{JieStarSCM4.TypeName} Device");

        builder.RegisterDevice<JieStarSCM8>()
            .WithDeviceFactory(JieStarSCM8.Device1, $"{JieStarSCM8.TypeName} Device 1")
            .WithDeviceFactory(JieStarSCM8.Device2, $"{JieStarSCM8.TypeName} Device 2")
            .WithDeviceFactory(JieStarSCM8.Device3, $"{JieStarSCM8.TypeName} Device 3");
    }
}
