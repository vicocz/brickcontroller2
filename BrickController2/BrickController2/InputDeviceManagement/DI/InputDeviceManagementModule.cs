using Autofac;
using BrickController2.PlatformServices.InputDeviceService;
using System.Collections.Generic;

namespace BrickController2.InputDeviceManagement.DI
{
    public class InputDeviceManagementModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InputDeviceManagerService>().As<IInputDeviceManagerService>().As<IInputDeviceEventServiceInternal>().As<IInputDeviceEventService>().SingleInstance()
                .OnActivated(e =>
                {
                    // resolve all registered input device services to trigger their registration
                    e.Context.Resolve<IEnumerable<IInputDeviceService>>();
                });
        }
    }
}
