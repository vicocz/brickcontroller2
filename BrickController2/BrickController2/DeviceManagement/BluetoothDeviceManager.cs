using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using static BrickController2.Protocols.BluetoothLowEnergy;

namespace BrickController2.DeviceManagement
{
    internal class BluetoothDeviceManager : IBluetoothDeviceManager
    {
        private readonly IBluetoothLEService _bleService;
        private readonly IEnumerable<IBluetoothLEDeviceManager> _bleDeviceManagers;
        private readonly AsyncLock _asyncLock = new AsyncLock();

        public BluetoothDeviceManager(
            IBluetoothLEService bleService,
            IEnumerable<IBluetoothLEDeviceManager> bleDeviceManagers)
        {
            _bleService = bleService;
            _bleDeviceManagers = bleDeviceManagers;
        }

        public bool IsBluetoothLESupported => _bleService.IsBluetoothLESupported;
        public bool IsBluetoothOn => _bleService.IsBluetoothOn;

        public async Task<bool> ScanAsync(Func<DeviceType, string, string, byte[]?, Task> deviceFoundCallback, CancellationToken token)
        {
            using (await _asyncLock.LockAsync())
            {
                if (!IsBluetoothOn)
                {
                    return true;
                }

                try
                {
                    return await _bleService.ScanDevicesAsync(
                        async scanResult =>
                        {
                            var deviceInfo = GetDeviceIfo(scanResult);
                            if (deviceInfo.DeviceType != DeviceType.Unknown)
                            {
                                await deviceFoundCallback(deviceInfo.DeviceType, deviceInfo.DeviceName, deviceInfo.DeviceAddress, deviceInfo.ManufacturerData);
                            }
                            else
                            {
                                
                            }
                        },
                        token);
                }
                catch (OperationCanceledException)
                {
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        // JK: The Scan-Response from the CaDA devices don't include a DeviceName and the DeviceAddress is changing with each response
        // So I had to extend the returned structure to be able to set these values.
        private (DeviceType DeviceType, string DeviceName, string DeviceAddress, byte[]? ManufacturerData) GetDeviceIfo(ScanResult scanResult)
        {
            string newDeviceName = scanResult.DeviceName;
            string newDeviceAddress = scanResult.DeviceAddress;

            IDictionary<byte, byte[]> advertismentData = scanResult.AdvertismentData;
            if (advertismentData == null)
            {
                return (DeviceType.Unknown, newDeviceName, newDeviceAddress, null);
            }

            if (!advertismentData.TryGetValue(ADTYPE_MANUFACTURER_SPECIFIC, out var manufacturerData) || manufacturerData.Length < 2)
            {
                var result = GetDeviceInfoByService(advertismentData);
                return (result.DeviceType, newDeviceName, newDeviceAddress, result.ManufacturerData);
            }

            var manufacturerDataString = BitConverter.ToString(manufacturerData).ToLower();
            var manufacturerId = manufacturerDataString.Substring(0, 5);

            switch (manufacturerId)
            {
                case "98-01": return (DeviceType.SBrick, newDeviceName, newDeviceAddress, manufacturerData);
                case "48-4d": return (DeviceType.BuWizz, newDeviceName, newDeviceAddress, manufacturerData);
                case "4e-05":
                    if (advertismentData.TryGetValue(ADTYPE_LOCAL_NAME_COMPLETE, out byte[]? completeLocalName))
                    {
                        var completeLocalNameString = BitConverter.ToString(completeLocalName).ToLower();
                        if (completeLocalNameString == "42-75-57-69-7a-7a") // BuWizz
                        {
                            return (DeviceType.BuWizz2, newDeviceName, newDeviceAddress, manufacturerData);
                        }
                        else
                        {
                            return (DeviceType.BuWizz3, newDeviceName, newDeviceAddress, manufacturerData);
                        }
                    }
                    break;
                case "05-45": // BuWizz2 has new ID since firmware 1.2.30
                    if (advertismentData.TryGetValue(ADTYPE_LOCAL_NAME_COMPLETE, out byte[]? buwizzName))
                    {
                        var completeLocalNameString = BitConverter.ToString(buwizzName).ToLower();
                        if (completeLocalNameString == "42-75-57-69-7a-7a-32") // BuWizz2
                        {
                            return (DeviceType.BuWizz2, newDeviceName, newDeviceAddress, manufacturerData);
                        }
                    }
                    break;
                case "97-03":
                    if (manufacturerDataString.Length >= 11)
                    {
                        var pupType = manufacturerDataString.Substring(9, 2);
                        switch (pupType)
                        {
                            case "40": return (DeviceType.Boost, newDeviceName, newDeviceAddress, manufacturerData);
                            case "41": return (DeviceType.PoweredUp, newDeviceName, newDeviceAddress, manufacturerData);
                            case "80": return (DeviceType.TechnicHub, newDeviceName, newDeviceAddress, manufacturerData);
                            case "84": return (DeviceType.TechnicMove, newDeviceName, newDeviceAddress, manufacturerData);
                            case "20": return (DeviceType.DuploTrainHub, newDeviceName, newDeviceAddress, manufacturerData);
                        }
                    }
                    break;
                case "33-ac": return (DeviceType.MK_DIY, newDeviceName, newDeviceAddress, manufacturerData);
            }

            // JK: by putting all the above code in specific managers (i.e. LegoDeviceManager, BuwizzDevicerManager,...)
            // we can decentralize the code...

            DeviceType deviceType = DeviceType.Unknown;
            if(_bleDeviceManagers.Any(c => c.TryGetDevice(manufacturerId, manufacturerData, out deviceType, ref newDeviceName, ref newDeviceAddress)))
            {
                return (deviceType, newDeviceName, newDeviceAddress, manufacturerData);
            }

            return (DeviceType.Unknown, newDeviceName, newDeviceAddress, null);
        }

        private (DeviceType DeviceType, byte[]? ManufacturerData) GetDeviceInfoByService(IDictionary<byte, byte[]> advertismentData)
        {
            // 0x06: 128 bits Service UUID type
            if (!advertismentData.TryGetValue(ADTYPE_SERVICE_128BIT, out byte[]? serviceData) || serviceData.Length < 16)
            {
                return (DeviceType.Unknown, null);
            }

            var serviceGuid = serviceData.GetGuid();

            switch (serviceGuid)
            {
                case var service when service == CircuitCubeDevice.SERVICE_UUID:
                    return (DeviceType.CircuitCubes, null);

                case var service when service == Wedo2Device.SERVICE_UUID:
                    return (DeviceType.WeDo2, null);

                default:
                    return (DeviceType.Unknown, null);
            };
        }
    }
}