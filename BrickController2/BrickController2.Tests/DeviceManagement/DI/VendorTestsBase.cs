using Autofac;
using BrickController2.DeviceManagement;
using BrickController2.UI.Services.AppIdentifier;
using Moq;

namespace BrickController2.Tests.DeviceManagement.DI;

public class VendorTestsBase
{
    protected virtual ContainerBuilder InitializeContainer()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterInstance(Mock.Of<IDeviceRepository>());

        Mock<IAppIdentifierService> appIdentifierService = new();
        appIdentifierService.Setup(x => x.GetAppId(2)).Returns(new byte[] { 0x01, 0x02 }); // MouldKing needs an AppId of 2 Bytes
        appIdentifierService.Setup(x => x.GetAppId(3)).Returns(new byte[] { 0x01, 0x02, 0x03 }); // CaDA needs an AppId of 3 Bytes
        builder.RegisterInstance(appIdentifierService.Object);

        builder.Register<DeviceFactory>(c =>
        {
            IComponentContext ctx = c.Resolve<IComponentContext>();
            return (deviceType, name, address, deviceData, settings) => ctx.ResolveOptionalKeyed<Device>(deviceType,
                new NamedParameter("name", name),
                new NamedParameter("address", address),
                new NamedParameter("deviceData", deviceData),
                new NamedParameter("settings", settings));
        });

        return builder;
    }
}
