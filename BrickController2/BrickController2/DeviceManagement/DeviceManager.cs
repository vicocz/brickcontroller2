using BrickController2.Helpers;
using BrickController2.UI.Services.MainThread;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class DeviceManager : NotifyPropertyChangedSource, IDeviceManager
    {
        private readonly IBluetoothDeviceManager _bluetoothDeviceManager;
        private readonly IInfraredDeviceManager _infraredDeviceManager;
        private readonly IDeviceRepository _deviceRepository;
        private readonly DeviceFactory _deviceFactory;
        private readonly IMainThreadService _uiThreadService;
        private readonly ILogger _logger;

        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly AsyncLock _foundDeviceLock = new AsyncLock();

        private bool _isScanning;

        public DeviceManager(
            IBluetoothDeviceManager bluetoothDeviceManager,
            IInfraredDeviceManager infraredDeviceManager,
            IDeviceRepository deviceRepository,
            DeviceFactory deviceFactory,
            IMainThreadService uiThreadService,
            ILogger<DeviceManager> logger)
        {
            _bluetoothDeviceManager = bluetoothDeviceManager;
            _infraredDeviceManager = infraredDeviceManager;
            _deviceRepository = deviceRepository;
            _deviceFactory = deviceFactory;
            _uiThreadService = uiThreadService;
            _logger = logger;
        }

        public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();

        public bool IsScanning
        {
            get { return _isScanning; }
            set { _isScanning = value; RaisePropertyChanged(); }
        }

        public bool IsBluetoothOn => _bluetoothDeviceManager.IsBluetoothOn;

        public async Task LoadDevicesAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                Devices.Clear();

                var deviceDTOs = await _deviceRepository.GetDevicesAsync();
                foreach (var deviceDTO in deviceDTOs)
                {
                    var device = _deviceFactory(deviceDTO.DeviceType, deviceDTO.Name, deviceDTO.Address, deviceDTO.DeviceData);
                    if (device != null)
                    {
                        Devices.Add(device);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to load device [DeviceType:{deviceType}, Name:{name}, Address:{address}]",
                            deviceDTO.DeviceType, deviceDTO.Name, deviceDTO.Address);
                    }
                }
            }
        }

        public async Task<bool> ScanAsync(CancellationToken token)
        {
            using (await _asyncLock.LockAsync())
            {
                IsScanning = true;

                try
                {
                    var infraScan = _infraredDeviceManager.ScanAsync(FoundDevice!, token);
                    var bluetoothScan = _bluetoothDeviceManager.ScanAsync(FoundDevice!, token);

                    await Task.WhenAll(infraScan, bluetoothScan);

                    return infraScan.Result && bluetoothScan.Result;
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    IsScanning = false;
                }
            }

            async Task FoundDevice(DeviceType deviceType, string deviceName, string deviceAddress, byte[] deviceData)
            {
                using (await _foundDeviceLock.LockAsync())
                {
                    if (Devices.Any(d => d.DeviceType == deviceType && d.Address == deviceAddress))
                    {
                        return;
                    }

                    var device = _deviceFactory(deviceType, deviceName, deviceAddress, deviceData);
                    if (device != null)
                    {
                        await _deviceRepository.InsertDeviceAsync(device.DeviceType, device.Name, device.Address, deviceData);

                        await _uiThreadService.RunOnMainThread(() => Devices.Add(device));
                    }
                }
            }
        }

        public Device? GetDeviceById(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Empty device ID was provided.");
                return null;
            }

            var deviceTypeAndAddress = id.Split('#');
            if (!Enum.TryParse<DeviceType>(deviceTypeAndAddress[0], out var deviceType))
            {
                _logger.LogWarning("Device ID [{id}] contains unsupported DeviceType:{deviceType}.",
                    id, deviceTypeAndAddress[0]);
                return null;
            }

            var deviceAddress = deviceTypeAndAddress[1];
            return Devices.FirstOrDefault(d => d.DeviceType == deviceType && d.Address == deviceAddress);
        }

        public async Task DeleteDeviceAsync(Device device)
        {
            using (await _asyncLock.LockAsync())
            {
                await _deviceRepository.DeleteDeviceAsync(device.DeviceType, device.Address);
                Devices.Remove(device);
            }
        }

        public async Task DeleteDevicesAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                await _deviceRepository.DeleteDevicesAsync();
                Devices.Clear();
            }
        }
    }
}
