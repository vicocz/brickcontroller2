using System.Collections.Generic;
using Autofac;
using BrickController2.DeviceManagement;
using BrickController2.Settings;

namespace BrickController2.Extensions;

public static class ContainerBuilderExtensions
{
    public static void RegisterDeviceFactory(this ContainerBuilder builder, DeviceType deviceType, string name, string address, byte[] deviceData, IEnumerable<NamedSetting> settings)
    {
        builder.Register(c => new DeviceFactoryData(deviceType, name, address, deviceData, settings)).As<IDeviceFactoryData>();
    }
}
