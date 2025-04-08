using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static BrickController2.Protocols.PfxProtocol;

namespace BrickController2.DeviceManagement
{
    internal class PfxBrickDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 5;

        private static readonly Guid SERVICE_UUID = new("49535343-fe7d-4ae5-8fa9-9fafd205e455");
        private static readonly Guid CHARACTERISTIC_UUID_WRITE = new("49535343-8841-43f4-a8d4-ecbe34729bb3");
        private static readonly Guid CHARACTERISTIC_UUID_NOTIFY = new("49535343-1e4d-4bd9-ba61-23c647249616");

        private readonly int[] _outputValues = new int[10];
        private readonly int[] _lastOutputValues = new int[10];
        private readonly object _outputLock = new object();

        private readonly ManualResetEventSlim _characteristicNotificationResetEvent = new ManualResetEventSlim();

        private volatile int _sendAttemptsLeft;

        private IGattCharacteristic? _writeCharacteristic;
        private IGattCharacteristic? _notifyCharacteristic;

        public PfxBrickDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.PfxBrick;
        public override int NumberOfChannels => 10;

        public override string BatteryVoltageSign => "V";
        protected override bool AutoConnectOnFirstConnect => false;

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            // Per channel range - percent
            var intValue = (int)(value * 100);

            lock (_outputLock)
            {
                if (_outputValues[channel] != intValue)
                {
                    _outputValues[channel] = intValue;
                    _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                }
            }
        }

        protected override async Task<bool> ValidateServicesAsync(IEnumerable<IGattService>? services, CancellationToken token)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
            _writeCharacteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_WRITE);

            _notifyCharacteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_NOTIFY);
            if (_notifyCharacteristic is not null)
            {
                await _bleDevice!.EnableNotificationAsync(_notifyCharacteristic, token);
            }

            return _writeCharacteristic is not null;
        }

        protected override void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
        {
            if (characteristicGuid != _notifyCharacteristic!.Uuid || data.Length == 0)
                return;

            if (data.Length == 1) // notification
            {
                Debug.WriteLine("Notification: " + data[0]);
                _characteristicNotificationResetEvent.Set();
            }
            else if (data.Length == 48) // status
            {

                var status = data[1];
                var error = data[2];

                HardwareVersion = $"{data[7]:X2}{data[8]:X2}"; // product_id
                FirmwareVersion = $"{data[37]:x2}.{data[38]:x2}"; // firmware_ver
            }
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
                lock (_outputLock)
                {
                    Array.Clear(_outputValues, 0, _outputValues.Length);
                    Array.Clear(_lastOutputValues, 0, _lastOutputValues.Length);
                    _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                }

                int[] values = new int[NumberOfChannels];
                int sendAttemptsLeft;

                while (!token.IsCancellationRequested)
                {
                    lock (_outputLock)
                    {
                        _outputValues.CopyTo(values, 0);

                        sendAttemptsLeft = _sendAttemptsLeft;
                        _sendAttemptsLeft = sendAttemptsLeft > 0 ? sendAttemptsLeft - 1 : 0;
                    }

                    if (!values.SequenceEqual(_lastOutputValues) || sendAttemptsLeft > 0)
                    {
                        if (await SendOutputValuesAsync(values, token).ConfigureAwait(false))
                        {
                            values.CopyTo(_lastOutputValues, 0);

                            lock (_outputLock)
                            {
                                _sendAttemptsLeft = 0;
                            }
                        }
                        await Task.Delay(5, token).ConfigureAwait(false);
                    }
                    else
                    {
                        await Task.Delay(10, token).ConfigureAwait(false);
                    }
                }
                // ensure everything is stopped in the end
                await WriteCommandAsync(PfxProtocol.AllOff(), token).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        private async Task<bool> SendOutputValuesAsync(int[] values, CancellationToken token)
        {
            try
            {
                var v0 = values[0];
                var v1 = values[1];

                // optimize writes
                if (v0 == v1)
                {
                    var motorCmd = PfxProtocol.SetMotorSpeed(MOTOR_OUTPUT_AB, v0);
                    return await WriteCommandAsync(motorCmd, token).ConfigureAwait(false);
                }
                else
                {
                    var motorCmd1 = PfxProtocol.SetMotorSpeed(MOTOR_OUTPUT_A, v0);
                    await WriteCommandAsync(motorCmd1, token).ConfigureAwait(false);

                    var motorCmd2 = PfxProtocol.SetMotorSpeed(MOTOR_OUTPUT_B, v1);
                    return await WriteCommandAsync(motorCmd2, token).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> WriteCommandAsync(byte[] command, CancellationToken token)
        {
            try
            {
                _characteristicNotificationResetEvent.Reset();

                // split per 20 bytes
                foreach (var cmdChunk in command.Chunk(20))
                {
                    var result = await _bleDevice!.WriteNoResponseAsync(_writeCharacteristic!, cmdChunk, token);
                }
                Debug.WriteLine("Cmd" + Convert.ToHexString(command));

                return await _characteristicNotificationResetEvent.WaitAsync(token).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task ReadDeviceInfo(CancellationToken token)
        {
            // request status update
            await _bleDevice!.WriteAsync(_writeCharacteristic!, PfxProtocol.GetStatus(), token);
        }
    }
}