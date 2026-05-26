using Autofac;
using BrickController2.Extensions;
using BrickController2.InputDeviceManagement.Sensors;
using BrickController2.PlatformServices.InputDeviceService;

namespace BrickController2.InputDeviceManagement.DI
{
    public class InputDeviceManagementModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InputDeviceManagerService>().As<IInputDeviceManagerService>().As<IInputDeviceEventServiceInternal>().As<IInputDeviceEventService>().SingleInstance();

            // generic sensor based input device
            builder.RegisterInputDeviceService<InputSensorService, OrientationSensorController>();
        }
    }
}
