using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Droid.PlatformServices.BluetoothLE
{
    public class BluetoothLEService : IBluetoothLEService
    {
        private readonly Context _context;
        private readonly BluetoothAdapter? _bluetoothAdapter;
        private readonly IEnumerable<IBluetoothLEAdvertiserDeviceScanData> _bluetoothLEAdvertiserDeviceScanDataList;

        private bool _isScanning = false;

        public BluetoothLEService(
            Context context,
            IEnumerable<IBluetoothLEAdvertiserDeviceScanData> bluetoothLEAdvertiserDeviceScanDataList)
        {
            _context = context;
            _bluetoothLEAdvertiserDeviceScanDataList = bluetoothLEAdvertiserDeviceScanDataList;

            if (context.PackageManager?.HasSystemFeature(PackageManager.FeatureBluetoothLe) ?? false)
            {
                var bluetoothManager = (BluetoothManager?)context.GetSystemService(Context.BluetoothService);
                _bluetoothAdapter = bluetoothManager?.Adapter;
            }
            else
            {
                _bluetoothAdapter = null;
            }
        }

        public bool IsBluetoothLESupported => _bluetoothAdapter != null;
        public bool IsBluetoothLEAdvertisingSupported => _bluetoothAdapter?.BluetoothLeAdvertiser != null;
        public bool IsBluetoothOn => _bluetoothAdapter?.IsEnabled ?? false;

        public async Task<bool> ScanDevicesAsync(Action<BrickController2.PlatformServices.BluetoothLE.ScanResult> scanCallback, CancellationToken token)
        {
            if (!IsBluetoothLESupported || !IsBluetoothOn || _isScanning)
            {
                return false;
            }

            try
            {
                var scanTaskList = new List<Task<bool>>();

                _isScanning = true;
                scanTaskList.Add(ScanAsync(scanCallback, token));
                scanTaskList.Add(ScanAdvertisingDeviceAsync(scanCallback, token));

                await Task.WhenAll(scanTaskList);

                return scanTaskList.All(scanTask => scanTask.Result);
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _isScanning = false;
            }
        }

        public IBluetoothLEDevice? GetKnownDevice(string address)
        {
            if (!IsBluetoothLESupported || _bluetoothAdapter is null)
            {
                return null;
            }

            return new BluetoothLEDevice(_context, _bluetoothAdapter, address);
        }

        private async Task<bool> ScanAsync(Action<BrickController2.PlatformServices.BluetoothLE.ScanResult> scanCallback, CancellationToken token)
        {
            try
            {
                var leScanner = new BluetoothLEScanner(scanCallback);
                var settingsBuilder = new ScanSettings.Builder()?
                    .SetCallbackType(ScanCallbackType.AllMatches)?
                    .SetScanMode(global::Android.Bluetooth.LE.ScanMode.LowLatency);

                _bluetoothAdapter?.BluetoothLeScanner?.StartScan(null, settingsBuilder?.Build(), leScanner);

                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                using (token.Register(() =>
                {
                    _bluetoothAdapter?.BluetoothLeScanner?.StopScan(leScanner);
                    tcs.TrySetResult(true);
                }))
                {
                    return await tcs.Task;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> ScanAdvertisingDeviceAsync(Action<BrickController2.PlatformServices.BluetoothLE.ScanResult> scanCallback, CancellationToken token)
        {
            var scanTaskList = new List<Task<bool>>();

            foreach (var currentEntry in _bluetoothLEAdvertiserDeviceScanDataList)
            {
                scanTaskList.Add(Task.Run(async () =>
                {
                    IBluetoothLEAdvertiserDevice? advertiserDevice = null;
                    try
                    {
                        if ((advertiserDevice = this.CreateBluetoothLEAdvertiserDevice()) != null)
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

        public IBluetoothLEAdvertiserDevice? CreateBluetoothLEAdvertiserDevice()
        {
            BluetoothLeAdvertiser? advertiser = _bluetoothAdapter?.BluetoothLeAdvertiser;

            if (advertiser != null)
            {
                return new BluetoothLEAdvertiserDevice(advertiser);
            }
            else
            {
                return null;
            }
        }
    }
}