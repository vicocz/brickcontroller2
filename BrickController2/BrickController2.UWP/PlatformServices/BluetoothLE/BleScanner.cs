using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Windows.Extensions;
using Windows.Devices.Bluetooth.Advertisement;

namespace BrickController2.Windows.PlatformServices.BluetoothLE
{
    public class BleScanner 
    {
        private readonly Action<ScanResult> _scanCallback;
        private readonly ConcurrentDictionary<ulong, string> _deviceNameCache;

        private readonly BluetoothLEAdvertisementWatcher _passiveWatcher;
        private readonly BluetoothLEAdvertisementWatcher _activeWatcher;

        public BleScanner(Action<ScanResult> scanCallback)
        {
            _scanCallback = scanCallback;
            _deviceNameCache = new ConcurrentDictionary<ulong, string>();

            // use passive advertisment for name resolution
            _passiveWatcher = new BluetoothLEAdvertisementWatcher { ScanningMode = BluetoothLEScanningMode.Passive };

            _passiveWatcher.Received += _passiveWatcher_Received;
            _passiveWatcher.Stopped += _passiveWatcher_Stopped;

            // use active scanner as ScanResult advertisment processor
            // because SBrick contains large manufacture data which may not come in single packet with device name
            _activeWatcher = new BluetoothLEAdvertisementWatcher { ScanningMode = BluetoothLEScanningMode.Active };
            _activeWatcher.Received += _activeWatcher_Received;
        }

        public void Start()
        {
            _passiveWatcher.Start();

            // setup ScanResult required data
            _activeWatcher.AdvertisementFilter.BytePatterns.Add(new BluetoothLEAdvertisementBytePattern
            {
                DataType = BluetoothLEAdvertisementDataTypes.ManufacturerSpecificData
            });
            _activeWatcher.Start();
        }

        private void _passiveWatcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            // simply update device name cache - if valid
            var deviceName = args.GetLocalName();
            if (deviceName.IsValidDeviceName())
            {
                _deviceNameCache.AddOrUpdate(args.BluetoothAddress, deviceName, (key, oldValue) => deviceName);
            }
        }

        private void _passiveWatcher_Stopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
        {
            _deviceNameCache.Clear();
        }

        private void _activeWatcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            if (!args.CanCarryData())
            {
                return;
            }
            // prefer local name if set, otherwise use cache (where only valid names can be)
            string deviceName = args.GetLocalName();
            if (!deviceName.IsValidDeviceName() && !_deviceNameCache.TryGetValue(args.BluetoothAddress, out deviceName))
            {
                return;
            }

            var bluetoothAddress = args.BluetoothAddress.ToBluetoothAddressString();

            var manufacturerSpecificData = args.Advertisement.GetSectionsByType(BluetoothLEAdvertisementDataTypes.ManufacturerSpecificData);
            var advertismentData = GetAdvertismentData(manufacturerSpecificData);

            _scanCallback(new ScanResult(deviceName, bluetoothAddress, advertismentData));
        }

        private static IDictionary<byte, byte[]> GetAdvertismentData(IReadOnlyCollection<BluetoothLEAdvertisementDataSection> sections)
        {
            return sections.ToDictionary(s => s.DataType, s => s.Data.ToByteArray());
        }

        public void Stop()
        {
            _passiveWatcher.Stop();
            _activeWatcher.Stop();
        }
    }
}