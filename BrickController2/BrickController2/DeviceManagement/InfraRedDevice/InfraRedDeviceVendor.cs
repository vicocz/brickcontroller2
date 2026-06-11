using Autofac;
using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.Vendors;

namespace BrickController2.DeviceManagement.InfraredDevice;

/// <summary>
/// fake Vendor for infrared devices
/// </summary>
internal class InfraRedDeviceVendor : Vendor<InfraRedDeviceVendor>
{
    public override string VendorName => "IRDevice";

    protected override void Register(VendorBuilder<InfraRedDeviceVendor> builder)
    {
        // device manager
        builder.ContainerBuilder.RegisterType<InfraredDeviceManager>().As<IInfraredDeviceManager>().SingleInstance();

        // manually added devices
        builder.RegisterDevice<InfraredDevice>()
            .WithDeviceFactory($"{0}", $"PF Infra {1}", null)
            .WithDeviceFactory($"{1}", $"PF Infra {2}", null)
            .WithDeviceFactory($"{2}", $"PF Infra {3}", null)
            .WithDeviceFactory($"{3}", $"PF Infra {4}", null);
    }
}
