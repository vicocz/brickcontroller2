using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrickController2.DeviceManagement;

namespace BrickController2.Extensions;

public static class DeviceManagerExtensions
{
    internal static async Task CreateDevicesAsync(this IDeviceManager deviceManager, IEnumerable<IDeviceFactoryData> deviceFactoryDataList)
    {
        foreach (var item in deviceFactoryDataList)
        {
            await deviceManager.CreateDeviceAsync(item);
        }
    }

    internal static async Task CreateDeviceAsync(this IDeviceManager deviceManager, IDeviceFactoryData deviceFactoryData)
    {
        await deviceManager.CreateDeviceAsync(deviceFactoryData.DeviceType, deviceFactoryData.Name, deviceFactoryData.Address, deviceFactoryData.DeviceData);
    }

    internal static async Task DeleteDevicesAsync(this IDeviceManager deviceManager, IEnumerable<Device> devices)
    {
        foreach (var item in devices)
        {
            await deviceManager.DeleteDeviceAsync(item);
        }
    }

    internal static bool ContainsAnyOutputDevice(this IDeviceManager deviceManager)
        => deviceManager.Devices.Any(x => x.HasOutputChannel);
}
