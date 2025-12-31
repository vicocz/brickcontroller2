using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.DeviceManagement.IO;
using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;

using static BrickController2.DeviceManagement.Vengit.SBrickProtocol;

namespace BrickController2.DeviceManagement.Vengit
{
    internal class SBrickLightDevice : BluetoothDevice
    {
        private const int BANK_0_CHANNELS = 16;
        private const int BANK_1_CHANNELS = 8;

        private readonly OutputValuesGroup<byte> _bankOutputs0 = new(BANK_0_CHANNELS);
        private readonly OutputValuesGroup<byte> _bankOutputs1 = new(BANK_1_CHANNELS);

        private IGattCharacteristic? _firmwareRevisionCharacteristic;
        private IGattCharacteristic? _hardwareRevisionCharacteristic;
        private IGattCharacteristic? _remoteControlCharacteristic;

        public SBrickLightDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.SBrickLight;
        public override string BatteryVoltageSign => "V";
        public override int NumberOfChannels => BANK_0_CHANNELS + BANK_1_CHANNELS;
        protected override bool AutoConnectOnFirstConnect => false;

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            // for lights use 0-255 range
            var rawValue = (byte)(Math.Abs(value) * 255);

            if (channel >= BANK_0_CHANNELS)
            {
                int lightChannel = channel - BANK_0_CHANNELS;
                _bankOutputs1.SetOutput(lightChannel, rawValue);
            }
            else
            {
                _bankOutputs0.SetOutput(channel, rawValue);
            }
        }

        protected override Task<bool> ValidateServicesAsync(IEnumerable<IGattService>? services, CancellationToken token)
        {
            var deviceInformationService = services?.FirstOrDefault(s => s.Uuid == GattProtocol.DeviceInformationServiceUuid);
            _firmwareRevisionCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == GattProtocol.FirmwareRevisionCharacteristicUuid);
            _hardwareRevisionCharacteristic = deviceInformationService?.Characteristics?.FirstOrDefault(c => c.Uuid == GattProtocol.HardwareRevisionCharacteristicUuid);

            var remoteControlService = services?.FirstOrDefault(s => s.Uuid == SBrickProtocol.ServiceUuid);
            _remoteControlCharacteristic = remoteControlService?.Characteristics?.FirstOrDefault(c => c.Uuid == RemoteControlCharacteristicUuid);

            return Task.FromResult(
                _firmwareRevisionCharacteristic is not null &&
                _hardwareRevisionCharacteristic is not null &&
                _remoteControlCharacteristic is not null);
        }

        protected override async Task<bool> AfterConnectSetupAsync(bool requestDeviceInformation, CancellationToken token)
        {
            try
            {
                if (requestDeviceInformation)
                {
                    await ReadDeviceInfo(token).ConfigureAwait(false);
                }
            }
            catch { }

            return true;
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            try
            {
                // reset outputs
                _bankOutputs0.Initialize();
                _bankOutputs1.Initialize();

                while (!token.IsCancellationRequested)
                {
                    // process first bank 0
                    bool changed = await TryProcessChanges(_bankOutputs0, LIGHTS_FLAGS_APPLY | LIGHTS_FLAGS_BANK_0, token);

                    // process first bank 1
                    if (await TryProcessChanges(_bankOutputs1, LIGHTS_FLAGS_APPLY | LIGHTS_FLAGS_BANK_1, token))
                    {
                        changed = true;
                    }

                    if (!changed)
                    {
                        await Task.Delay(10, token).ConfigureAwait(false);
                    }
                }
            }
            catch
            {
            }
        }

        private async Task<bool> TryProcessChanges(OutputValuesGroup<byte> valueBank, byte flags, CancellationToken token)
        {
            try
            {
                if (valueBank.TryGetValues(out var values))
                {
                    var command = BuildSetAllLights(flags, values);
                    var success = await _bleDevice!.WriteAsync(_remoteControlCharacteristic!, command, token).ConfigureAwait(false);
                    if (success)
                    {
                        // confirm successfull sending
                        valueBank.Commmit();
                        await Task.Delay(5, token).ConfigureAwait(false);
                        return true;
                    }
                }
                return false;
            }
            catch
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

            var hardwareData = await _bleDevice.ReadAsync(_hardwareRevisionCharacteristic!, token);
            var hardwareVersion = hardwareData?.ToAsciiStringSafe();
            if (!string.IsNullOrEmpty(hardwareVersion))
            {
                HardwareVersion = hardwareVersion;
            }

            // 0x0F Query ADC | voltage on 0x08
            await _bleDevice.WriteAsync(_remoteControlCharacteristic!, [0x0f, 0x08], token);
            var voltageBuffer = await _bleDevice!.ReadAsync(_remoteControlCharacteristic!, token);
            if (voltageBuffer is not null && voltageBuffer.Length >= 2)
            {
                var rawVoltage = voltageBuffer[0] + (voltageBuffer[1] << 8);
                var voltage = (rawVoltage * 0.42567F) / 2047;
                BatteryVoltage = voltage.ToString("F2");
            }
        }
    }
}
