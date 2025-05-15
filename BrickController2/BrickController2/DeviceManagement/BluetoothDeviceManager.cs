using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;

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
                        if (TryGetDevice(scanResult, out var deviceInfo))
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

        private bool TryGetDevice(ScanResult scanResult, out FoundDevice device)
        {
            if (scanResult.AdvertismentData != null)
            {
                FoundDevice foundDevice = default;
                if (_bleDeviceManagers.Any(c => c.TryGetDevice(scanResult, out foundDevice)))
                {
                    device = foundDevice;
                    return true;
                }
            }

            device = default;
            return false;
        }
    }
}