using Autofac;
using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.Vendors;
using BrickController2.PlatformServices.Infrared;

namespace BrickController2.DeviceManagement.InfraredDevice;

/// <summary>
/// fake Vendor for infrared devices
/// </summary>
internal class InfraRedDeviceVendor : Vendor<InfraRedDeviceVendor>
{
    private IInfraredService? _infraredService;
    
    public override string VendorName => "IRDevice";

    public override bool IsAvailable => _infraredService?.IsInfraredSupported ?? false;

    protected override void Register(VendorBuilder<InfraRedDeviceVendor> builder)
    {
        // device manager
        builder.ContainerBuilder.RegisterType<InfraredDeviceManager>().As<IInfraredDeviceManager>().SingleInstance();

        builder.ContainerBuilder.RegisterBuildCallback(scope =>
        {
            _infraredService = scope.Resolve<IInfraredService>();
        });

        // manually added devices
        builder.RegisterDevice<InfraredDevice>()
            .WithDeviceFactory($"{0}", $"PF Infra {1}", null)
            .WithDeviceFactory($"{1}", $"PF Infra {2}", null)
            .WithDeviceFactory($"{2}", $"PF Infra {3}", null)
            .WithDeviceFactory($"{3}", $"PF Infra {4}", null);
    }
}
