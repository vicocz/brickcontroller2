using BrickController2.DeviceManagement.BuWizz;
using BrickController2.DeviceManagement.IO;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class BuWizzDevice : BluetoothDevice
    {

        private static readonly Guid SERVICE_UUID = new Guid("0000ffe0-0000-1000-8000-00805f9b34fb");
        private static readonly Guid CHARACTERISTIC_UUID = new Guid("0000ffe1-0000-1000-8000-00805f9b34fb");

        private static readonly TimeSpan LastOutputTimeout = TimeSpan.FromMilliseconds(1500);

        private const string DefaultOutputLevelName = "BuWizzDefaultOutputLevel";
        private const BuWizzOutputLevels DefaultLevel = BuWizzOutputLevels.Normal;

        private readonly OutputValuesGroup<int> _outputGroup = new(5);

        private IGattCharacteristic? _characteristic;

        public BuWizzDevice(string name, string address, IEnumerable<NamedSetting> settings, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
            // apply values (if any) or default
            SetSettingValue(DefaultOutputLevelName, settings, DefaultLevel);
            // update output value again to apply settings
            _outputLevel = DefaultOutputLevel;
        }

        public override DeviceType DeviceType => DeviceType.BuWizz;
        public override int NumberOfChannels => 4;
        public override int NumberOfOutputLevels => 3;
        public override int DefaultOutputLevel => (int)GetSettingValue(DefaultOutputLevelName, DefaultLevel);
        protected override bool AutoConnectOnFirstConnect => false;

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
            var outputLevelValue = Math.Max(0, Math.Min(NumberOfOutputLevels - 1, value));
            _outputGroup.SetOutput(4, outputLevelValue);
        }

        public override bool CanBePowerSource => true;

        protected override Task<bool> ValidateServicesAsync(IEnumerable<IGattService>? services, CancellationToken token)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
            _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID);

            return Task.FromResult(_characteristic is not null);
        }


        protected override void BeforeDisconnectCleanup()
        {
            // Clear cached characteristic reference to prevent using stale native Android objects on reconnection
            _characteristic = null;
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            _outputGroup.Initialize();
            _outputGroup.SetOutput(4, DefaultOutputLevel);

            DateTime lastOutputWrite = DateTime.MinValue;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_outputGroup.TryGetValues(out var values) || (DateTime.Now - lastOutputWrite > LastOutputTimeout))
                    {
                        if (await SendOutputValuesAsync(values[0], values[1], values[2], values[3], level: values[4], token).ConfigureAwait(false))
                        {
                            _outputGroup.Commit();
                            lastOutputWrite = DateTime.Now;
                        }
                    }
                    else
                    {
                        await Task.Delay(10, token).ConfigureAwait(false);
                    }
                }
                catch
                {
                }
            }
        }

        private async Task<bool> SendOutputValuesAsync(int v0, int v1, int v2, int v3, int level, CancellationToken token)
        {
            try
            {
                var sendOutputBuffer = new byte[]
                {
                    (byte)((Math.Abs(v0) >> 2) | (v0 < 0 ? 0x40 : 0) | 0x80),
                    (byte)((Math.Abs(v1) >> 2) | (v1 < 0 ? 0x40 : 0)),
                    (byte)((Math.Abs(v2) >> 2) | (v2 < 0 ? 0x40 : 0)),
                    (byte)((Math.Abs(v3) >> 2) | (v3 < 0 ? 0x40 : 0)),
                    (byte)(level * 0x20)
                };

                var result = await _bleDevice!.WriteNoResponseAsync(_characteristic!, sendOutputBuffer, token);
                await Task.Delay(60, token);
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}