using Autofac;
using BrickController2.CreationManagement.Sharing;

namespace BrickController2.CreationManagement.DI
{
    public class CreationManagementModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CreationRepository>().As<ICreationRepository>().SingleInstance();
            builder.RegisterType<CreationManager>().As<ICreationManager>().SingleInstance();
            builder.RegisterType<SharingManager<Creation>>().As<ISharingManager<Creation>>().SingleInstance();
            builder.RegisterType<SharingManager<Sequence>>().As<ISharingManager<Sequence>>().SingleInstance();
            builder.RegisterType<SharingManager<ControllerProfile>>().As<ISharingManager<ControllerProfile>>().SingleInstance();
        }
    }
}
