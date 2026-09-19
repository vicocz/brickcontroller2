using Autofac;
using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.Vendors;
using BrickController2.PlatformServices.Infrared;

namespace BrickController2.DeviceManagement.PowerFunctions;

/// <summary>
/// Vendor for Power Functions devices
/// </summary>
internal class PowerFunctions : Vendor<PowerFunctions>
{
    private IInfraredService? _infraredService;
    
    public override string VendorName => "LEGO";

    public override bool IsAvailable => _infraredService != null && _infraredService.IsInfraredSupported && _infraredService.IsCarrierFrequencySupported(PowerFunctionsManager.IR_FREQUENCY);

    protected override void Register(VendorBuilder<PowerFunctions> builder)
    {
        // device manager
        builder.ContainerBuilder.RegisterType<PowerFunctionsManager>().As<IPowerFunctionsManager>().SingleInstance();

        builder.ContainerBuilder.RegisterBuildCallback(scope =>
        {
            _infraredService = scope.Resolve<IInfraredService>();
        });

        // manually added devices
        builder.RegisterDevice<PowerFunctionsDevice>()
            .WithImages("powerfunctions_image.png", "powerfunctions_image_small.png")
            .WithDeviceFactory("0", "PF Infra 1")
            .WithDeviceFactory("1", "PF Infra 2")
            .WithDeviceFactory("2", "PF Infra 3")
            .WithDeviceFactory("3", "PF Infra 4");
    }
}
