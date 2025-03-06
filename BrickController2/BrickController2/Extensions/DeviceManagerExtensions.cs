using System.Collections.Generic;
using System.Threading.Tasks;
using BrickController2.DeviceManagement;

namespace BrickController2.Extensions;

public static class DeviceManagerExtensions
{
    internal static async Task CreateDevicesAsync(this IDeviceManager deviceManager, IEnumerable<IStaticDeviceFactoryData> staticDeviceFactoryDataList)
    {
        foreach (var item in staticDeviceFactoryDataList)
        {
            await deviceManager.CreateDeviceAsync(item);
        }
    }

    internal static async Task CreateDeviceAsync(this IDeviceManager deviceManager, IStaticDeviceFactoryData staticDeviceFactoryData)
    {
        await deviceManager.CreateDeviceAsync(staticDeviceFactoryData.DeviceType, staticDeviceFactoryData.Name, staticDeviceFactoryData.Address, staticDeviceFactoryData.DeviceData);
    }

    internal static async Task DeleteDevicesAsync(this IDeviceManager deviceManager, IEnumerable<Device> devices)
    {
        foreach (var item in devices)
        {
            await deviceManager.DeleteDeviceAsync(item);
        }
    }
}
