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
        private readonly IEnumerable<IBluetoothLEAdvertiserDeviceScanInfo> _bluetoothLEAdvertiserDeviceScanInfoList;
        private readonly AsyncLock _asyncLock = new AsyncLock();

        public BluetoothDeviceManager(
            IBluetoothLEService bleService,
            IEnumerable<IBluetoothLEDeviceManager> bleDeviceManagers,
            IEnumerable<IBluetoothLEAdvertiserDeviceScanInfo> bluetoothLEAdvertiserDeviceScanInfoList)
        {
            _bleService = bleService;
            _bleDeviceManagers = bleDeviceManagers;
            _bluetoothLEAdvertiserDeviceScanInfoList = bluetoothLEAdvertiserDeviceScanInfoList;
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
                    var scanTaskList = new List<Task<bool>>();
                    
                    scanTaskList.Add(ScanDeviceAsync(deviceFoundCallback, token));

                    if (_bleService.IsBluetoothLEAdvertisingSupported)
                    {
                        scanTaskList.Add(ScanAdvertisingDeviceAsync(token));
                    }

                    await Task.WhenAll(scanTaskList);
                    return scanTaskList.All(scanTask => scanTask.Result);
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
        private async Task<bool> ScanDeviceAsync(Func<DeviceType, string, string, byte[]?, Task> deviceFoundCallback, CancellationToken token)
        {
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

        private async Task<bool> ScanAdvertisingDeviceAsync(CancellationToken token)
        {
            var scanTaskList = new List<Task<bool>>();

            foreach (var currentEntry in _bluetoothLEAdvertiserDeviceScanInfoList)
            {
                scanTaskList.Add(Task.Run(async () =>
                {
                    IBluetoothLEAdvertiserDevice? advertiserDevice = null;
                    try
                    {
                        if ((advertiserDevice = _bleService.CreateBluetoothLEAdvertiserDevice()) != null)
                        {
                            await advertiserDevice.StartAdvertiseAsync(currentEntry.AdvertisingIterval, currentEntry.TXPowerLevel, currentEntry.ManufacturerId, currentEntry.CreateScanData());

                            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                            using (token.Register(async () =>
                            {
                                await advertiserDevice.StopAdvertiseAsync();

                                tcs.TrySetResult(true);
                            }))
                            {
                                return await tcs.Task;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                    finally
                    {
                        advertiserDevice?.Dispose();
                    }
                }));
            }

            await Task.WhenAll(scanTaskList);
            return scanTaskList.All(scanTask => scanTask.Result);
        }

        private FoundDevice GetDeviceIfo(ScanResult scanResult)
        {
            IDictionary<byte, byte[]> advertismentData = scanResult.AdvertismentData;
            if (advertismentData == null)
            {
                return FoundDevice.Unknown;
            }

            if (!advertismentData.TryGetValue(ADTYPE_MANUFACTURER_SPECIFIC, out var manufacturerData) || manufacturerData.Length < 2)
            {
                var result = GetDeviceInfoByService(advertismentData);
                return new FoundDevice(result.DeviceType, scanResult.DeviceName, scanResult.DeviceAddress, result.ManufacturerData);
            }

            var foundDevice = new FoundDevice(DeviceType.Unknown, scanResult.DeviceName, scanResult.DeviceAddress, manufacturerData);
            var manufacturerDataString = BitConverter.ToString(manufacturerData).ToLower();
            var manufacturerId = manufacturerDataString.Substring(0, 5);

            switch (manufacturerId)
            {
                case "98-01": return foundDevice with { DeviceType = DeviceType.SBrick };
                case "48-4d": return foundDevice with { DeviceType = DeviceType.BuWizz };
                case "4e-05":
                    if (advertismentData.TryGetValue(ADTYPE_LOCAL_NAME_COMPLETE, out byte[]? completeLocalName))
                    {
                        var completeLocalNameString = BitConverter.ToString(completeLocalName).ToLower();
                        if (completeLocalNameString == "42-75-57-69-7a-7a") // BuWizz
                        {
                            return foundDevice with { DeviceType = DeviceType.BuWizz2 };
                        }
                        else
                        {
                            return foundDevice with { DeviceType = DeviceType.BuWizz3 };
                        }
                    }
                    break;
                case "05-45": // BuWizz2 has new ID since firmware 1.2.30
                    if (advertismentData.TryGetValue(ADTYPE_LOCAL_NAME_COMPLETE, out byte[]? buwizzName))
                    {
                        var completeLocalNameString = BitConverter.ToString(buwizzName).ToLower();
                        if (completeLocalNameString == "42-75-57-69-7a-7a-32") // BuWizz2
                        {
                            return foundDevice with { DeviceType = DeviceType.BuWizz2 };
                        }
                    }
                    break;
                case "97-03":
                    if (manufacturerDataString.Length >= 11)
                    {
                        var pupType = manufacturerDataString.Substring(9, 2);
                        switch (pupType)
                        {
                            case "40": return foundDevice with { DeviceType = DeviceType.Boost };
                            case "41": return foundDevice with { DeviceType = DeviceType.PoweredUp };
                            case "80": return foundDevice with { DeviceType = DeviceType.TechnicHub };
                            case "84": return foundDevice with { DeviceType = DeviceType.TechnicMove };
                            case "20": return foundDevice with { DeviceType = DeviceType.DuploTrainHub };
                        }
                    }
                    break;
                case "33-ac": return foundDevice with { DeviceType = DeviceType.MK_DIY };
            }

            if(_bleDeviceManagers.Any(c => c.TryGetDevice(manufacturerId, manufacturerData, ref foundDevice)))
            {
                return foundDevice;
            }

            return FoundDevice.Unknown;
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