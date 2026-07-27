using BrickController2.DeviceManagement.BuWizz;
using BrickController2.DeviceManagement.IO;
using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static BrickController2.Protocols.GattProtocol;

namespace BrickController2.DeviceManagement
{
    internal class BuWizz2Device : BluetoothDevice, IDeviceType<BuWizz2Device>
    {
        internal static readonly Guid SERVICE_UUID = new Guid("4e050000-74fb-4481-88b3-9919b1676e93");
        private static readonly Guid CHARACTERISTIC_UUID = new Guid("000092d1-0000-1000-8000-00805f9b34fb");
                
        private static readonly TimeSpan VoltageMeasurementTimeout = TimeSpan.FromSeconds(5);

        private const string SwapChannelsSettingName = "BuWizz2SwapChannels";
        private const string DefaultOutputLevelName = "BuWizz2DefaultOutputLevel";
        private const BuWizz2OutputLevels DefaultLevel = BuWizz2OutputLevels.Normal;

        private readonly OutputValuesGroup<int> _outputGroup = new(4);

        private DateTime _batteryMeasurementTimestamp;
        private byte _batteryVoltageRaw;
        private byte _motorVoltageRaw;

        private volatile int _outputLevelValue;
        
        private IGattCharacteristic? _characteristic;
        private IGattCharacteristic? _modelNumberCharacteristic;
        private IGattCharacteristic? _firmwareRevisionCharacteristic;

        public BuWizz2Device(string name, string address, byte[] deviceData, IEnumerable<NamedSetting> settings, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
            // On BuWizz2 with manufacturer data 0x4e054257001e the ports are swapped
            // (no normal BuWizz2es manufacturer data is 0x4e054257001b)
            var swapChannels = deviceData != null && deviceData.Length >= 6 && deviceData[5] == 0x1E;

            // apply values (if any) or default
            SetSettingValue(SwapChannelsSettingName, settings, swapChannels);
            SetSettingValue(DefaultOutputLevelName, settings, DefaultLevel);
            // update output value again to apply settings
            _outputLevel = DefaultOutputLevel;
        }

        public static string TypeName => "BuWizz 2";
        public static DeviceType Type => DeviceType.BuWizz2;

        public override DeviceType DeviceType => DeviceType.BuWizz2;
        public override int NumberOfChannels => 4;
        public override int NumberOfOutputLevels => 4;
        public override int DefaultOutputLevel => (int)GetSettingValue(DefaultOutputLevelName, DefaultLevel);

        public override float AccelarationStep => 1.0f / 7.0f;

        protected override bool AutoConnectOnFirstConnect => false;

        public override string BatteryVoltageSign => "V";

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (int)(value * 255);
            _outputGroup.SetOutput(channel, intValue);
        }

        public override bool CanSetOutputLevel => true;

        public override void SetOutputLevel(int value)
        {
            _outputLevelValue = Math.Max(0, Math.Min(NumberOfOutputLevels - 1, value));
        }

        public override bool CanBePowerSource => true;

        protected override async Task<bool> ValidateServicesAsync(IEnumerable<IGattService>? services, CancellationToken token)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
            _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID);

            var deviceInformationService = services?.FirstOrDefault(s => s.Uuid == Services.DeviceInformation);
            _firmwareRevisionCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == Characteristics.FirmwareRevision);
            _modelNumberCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == Characteristics.ModelNumber);

            if (_characteristic is not null)
            {
                await _bleDevice!.EnableNotificationAsync(_characteristic, token);
            }

            return _characteristic is not null && _firmwareRevisionCharacteristic is not null && _modelNumberCharacteristic is not null;
        }

        protected override async ValueTask BeforeDisconnectAsync(CancellationToken token)
        {
            if (_characteristic != null && _bleDevice != null)
            {
                await _bleDevice.DisableNotificationAsync(_characteristic, token);
            }
        }

        protected override void BeforeDisconnectCleanup()
        {
            // Clear cached characteristic references to prevent using stale native Android objects on reconnection
            _characteristic = null;
            _modelNumberCharacteristic = null;
            _firmwareRevisionCharacteristic = null;
        }

        protected override void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
        {
            if (characteristicGuid != _characteristic?.Uuid || data.Length < 4 || data[0] != 0x00)
            {
                return;
            }

            // Byte 1: Status flags - Bits 3-4 Battery level status (0 - empty, motors disabled; 1 - low; 2 - medium; 3 - full) 

            // do some change filtering as data are comming at 25Hz frequency
            if (GetVoltage(data, out float batteryVoltage, out float motorVoltage))
            {
                BatteryVoltage = $"{batteryVoltage:F2} / {motorVoltage:F2}";
            }
        }

        private bool GetVoltage(byte[] data, out float batteryVoltage, out float motorVoltage)
        {
            // Byte 2: Battery voltage (3 V + value * 0,01 V) -range 3,00 V - 4,27 V
            // Byte 3: Output(motor) voltage(4 V + value * 0,05 V) - range 4,00 V - 16,75 V
            batteryVoltage = 3.0f + data[2] * 0.01f;
            motorVoltage = 4.0f + data[3] * 0.05f;

            const int delta = 2;

            if (Math.Abs(_batteryVoltageRaw - data[2]) > delta ||
                Math.Abs(_motorVoltageRaw - data[3]) > delta || 
                DateTime.Now - _batteryMeasurementTimestamp > VoltageMeasurementTimeout)
            {
                _batteryVoltageRaw = data[2];
                _motorVoltageRaw = data[3];

                _batteryMeasurementTimestamp = DateTime.Now;

                return true;
            }

            return false;
        }

        protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
        {
            try
            {
                if (requestDeviceInformation)
                {
                    await ReadDeviceInfo(token);
                }
            }
            catch { }

            return true;
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            try
            {
                _outputGroup.Initialize();
                _outputLevelValue = DefaultOutputLevel;

                var lastSentOutputLevelValue = -1;

                while (!token.IsCancellationRequested)
                {
                    if (lastSentOutputLevelValue != _outputLevelValue)
                    {
                        if (await SendOutputLevelValueAsync(_outputLevelValue, token))
                        {
                            lastSentOutputLevelValue = _outputLevelValue;
                        }
                    }
                    else if (_outputGroup.TryGetValues(out var values))
                    {
                        if (await SendOutputValuesAsync(values[0], values[1], values[2], values[3], token).ConfigureAwait(false))
                        {
                            _outputGroup.Commit();
                        }
                    }
                    else
                    {
                        await Task.Delay(10, token).ConfigureAwait(false);
                    }
                }
            }
            catch
            {
            }
        }

        private bool SwapChannels => base.GetSettingValue<bool>(SwapChannelsSettingName);

        private async Task<bool> SendOutputValuesAsync(int v0, int v1, int v2, int v3, CancellationToken token)
        {
            try
            {
                var sendOutputBuffer = new byte[] { 0x10, 0x00, 0x00, 0x00, 0x00, 0x00 };

                if (SwapChannels)
                {
                    sendOutputBuffer[1] = (byte)(v1 / 2);
                    sendOutputBuffer[2] = (byte)(v0 / 2);
                    sendOutputBuffer[3] = (byte)(v3 / 2);
                    sendOutputBuffer[4] = (byte)(v2 / 2);
                }
                else
                {
                    sendOutputBuffer[1] = (byte)(v0 / 2);
                    sendOutputBuffer[2] = (byte)(v1 / 2);
                    sendOutputBuffer[3] = (byte)(v2 / 2);
                    sendOutputBuffer[4] = (byte)(v3 / 2);
                }

                return await _bleDevice!.WriteAsync(_characteristic!, sendOutputBuffer, token);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> SendOutputLevelValueAsync(int outputLevelValue, CancellationToken token)
        {
            try
            {
                var sendOutputLevelBuffer = new byte[] { 0x11, (byte)(outputLevelValue + 1) };

                return await _bleDevice!.WriteAsync(_characteristic!, sendOutputLevelBuffer, token);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task ReadDeviceInfo(CancellationToken token)
        {
            var firmwareData = await _bleDevice!.ReadAsync(_firmwareRevisionCharacteristic!, token);
            var firmwareVersion = firmwareData?.ToAsciiStringSafe();
            if (!string.IsNullOrEmpty(firmwareVersion))
            {
                FirmwareVersion = firmwareVersion;
            }

            var modelNumberData = await _bleDevice!.ReadAsync(_modelNumberCharacteristic!, token);
            var modelNumber = modelNumberData?.ToAsciiStringSafe();
            if (!string.IsNullOrEmpty(modelNumber))
            {
                HardwareVersion = modelNumber;
            }
        }
    }
}
