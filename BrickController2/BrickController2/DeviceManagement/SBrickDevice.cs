using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class SBrickDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 4;

        private const byte MESSAGE_TYPE_PRODUCT_TYPE = 0x00;

        private static readonly Guid SERVICE_UUID_REMOTE_CONTROL = new Guid("4dc591b0-857c-41de-b5f1-15abda665b0c");
        private static readonly Guid CHARACTERISTIC_UUID_QUICK_DRIVE = new Guid("489a6ae0-c1ab-4c9c-bdb2-11d373c1b7fb");
        private static readonly Guid CHARACTERISTIC_UUID_REMOTE_CONTROL_COMMAND = new Guid("02b8cbcc-0e25-4bda-8790-a15f53e6010f");

        private readonly byte[] _sendBuffer = new byte[4];
        private readonly int[] _outputValues = new int[4];

        private volatile int _sendAttemptsLeft;

        private IGattCharacteristic _characteristic;
        private IGattCharacteristic _remoteCommandCharacteristic;

        public SBrickDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
            // process only ProductType
            var productTypeMessage = SliceDeviceData(deviceData, 2).FirstOrDefault(d => d.MessageType == MESSAGE_TYPE_PRODUCT_TYPE);
            if (productTypeMessage.MessageData != null)
            {
                ProcessProductTypeMessage(productTypeMessage.MessageData);
            }
        }

        public Version HardwareVersion
        {
            get;
            private set;
        }

        public override DeviceType DeviceType => DeviceType.SBrick;
        public override int NumberOfChannels => 4;
        protected override bool AutoConnectOnFirstConnect => false;

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (int)(value * 255);
            if (_outputValues[channel] == intValue)
            {
                return;
            }

            _outputValues[channel] = intValue;
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
        }

        protected override void RegisterDefaultPorts()
        {
            RegisterPorts(new[]
            {
                new DevicePort(0, "1"),
                new DevicePort(1, "2"),
                new DevicePort(2, "3"),
                new DevicePort(3, "4"),
            });
        }

        protected async override Task<bool> ValidateServicesAsync(IEnumerable<IGattService> services, CancellationToken token)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID_REMOTE_CONTROL);

            // Quick Commands for drive
            _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_QUICK_DRIVE);
            // use Remote control commands for subscriptions
            _remoteCommandCharacteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_REMOTE_CONTROL_COMMAND);

            if (_characteristic != null && _remoteCommandCharacteristic != null)
            {
                if (await _bleDevice.EnableNotificationAsync(_characteristic, token))
                {
                    // Use the command "2C Set up periodic voltage measurement" to measure port pins on SBrick Plus(hardwareversion 11 and 13) models.
                    await _bleDevice?.WriteAsync(_remoteCommandCharacteristic, new byte[] { 0x2C, 0x08, 0x09 }, token);
                    await _bleDevice?.WriteAsync(_remoteCommandCharacteristic, new byte[] { 0x2e, 0x08, 0x09 }, token);

                    return true;
                }
            }
            return false;
        }

        private IEnumerable<(byte MessageType, IReadOnlyList<byte> MessageData)> SliceDeviceData(byte[] deviceData, int startIndex = 0)
        {
            var index = startIndex;
            while (index < deviceData.Length)
            {
                var messageLength = deviceData[index];
                var messageType = deviceData[index + 1];

                yield return (messageType, new ArraySegment<byte>(deviceData, index + 2, messageLength - 1));

                index += messageLength + 1;
            }
        }

        private void ProcessMessage(byte messageType, IReadOnlyList<byte> messageData)
        {
            switch (messageType)
            {
                case MESSAGE_TYPE_PRODUCT_TYPE:
                    ProcessProductTypeMessage(messageData);
                    break;

                case 0x01:
                    ProcessLegacySensorRawReading(messageData);
                    break;

                case 0x04:
                    ProcessStatus(messageData);
                    break;

                case 0x06:
                    ProcessVoltageMeasurement(messageData);
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine($"MessageType: {messageType} UNKNOWN");
                    break;
            }
        }

        private void ProcessProductTypeMessage(IReadOnlyList<byte> messageData)
        {
            //Product type MSG
            // 0x00 = Product IDs
            // 0x05 = Hardware version MAJOR
            // 0x00 = Hardware version MINOR
            // 0x05 = FIRMWARE version MAJOR
            // 0x19 = FIRMWARE version MINOR

            if (messageData.Count >= 3)
            {
                HardwareVersion = new Version(messageData[1], messageData[2]);
            }

            if (messageData.Count >= 5)
            {
                FirmwareVersion = new Version(messageData[3], messageData[4]);
            }
        }

        private void ProcessLegacySensorRawReading(IReadOnlyList<byte> messageData)
        {
            // 01 BlueGiga ADC sensor raw reading
            // 0x00 / 0x0e = battery reading /  internaltemperature sensor
            // 0xXX + 0xYY = ADC data

            var channelType = messageData[0];
            ushort value = readUInt16LE(messageData, 1);

            switch (channelType)
            {
                //“04 01 00 12 F0” - battery reading '12f0' on SBrick
                // VPSU = (ADC * 0.83875) / 2047.0
                case 0x00:
                    Voltage = (value * 0.83875f) / 2047.0f;
                    break;

                //“04 01 0e 12 F0” - temperature reading '12f0
                case 0x0e:
                    var temperature = value / 118.85795 - 160;
                    break;
            }

        }

        private void ProcessStatus(IReadOnlyList<byte> messageData)
        {
            var status = messageData[0];

            System.Diagnostics.Debug.WriteLine($"Status: {status}");
        }

        private void ProcessVoltageMeasurement(IReadOnlyList<byte> messageData)
        {
            // 06 Voltage measurement
            // (N-1) - Measurement data - 

            // Measurement data may contain measurements over multiple channels. Each measurementis described over 2 bytes.
            // The 3 upper nibbles contain the 12 bit raw ADC data. The low nibble contains the channel number.

            var index = 0;

            while (index < messageData.Count)
            {
                var rawValue = readUInt16LE(messageData, index);

                var channelNumber = rawValue & 0x0F;
                var adcValue = rawValue >> 4;

                switch (channelNumber)
                {
                    //8 - Battery voltage
                    case 0x08:
                        Voltage = adcValue / 2.85f ;
                        BatteryLevel = adcValue / 4092f * 100f * 2.6f;
                        break;
                    //9 - Internal temperature
                    case 0x09:
                        var temperature = adcValue / 118.85795 - 160;
                        break;
                }
                index += 2;
            }
        }

        private static ushort readUInt16LE(IReadOnlyList<byte> data, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt16(new byte[] { data[startIndex], data[startIndex + 1] }, 0);
            }

            return BitConverter.ToUInt16(new byte[] { data[startIndex + 1], data[startIndex] }, 0);
        }

        protected override void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
        {
            foreach (var (MessageType, MessageData) in SliceDeviceData(data))
            {
                ProcessMessage(MessageType, MessageData);
            }
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            try
            {
                _outputValues[0] = 0;
                _outputValues[1] = 0;
                _outputValues[2] = 0;
                _outputValues[3] = 0;
                _sendAttemptsLeft = MAX_SEND_ATTEMPTS;

                while (!token.IsCancellationRequested)
                {
                    if (_sendAttemptsLeft > 0)
                    {
                        int v0 = _outputValues[0];
                        int v1 = _outputValues[1];
                        int v2 = _outputValues[2];
                        int v3 = _outputValues[3];

                        if (await SendOutputValuesAsync(v0, v1, v2, v3, token))
                        {
                            if (v0 != 0 || v1 != 0 || v2 != 0 || v3 != 0)
                            {
                                _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                            }
                            else
                            {
                                _sendAttemptsLeft--;
                            }
                        }
                        else
                        {
                            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                        }
                    }
                    else
                    {
                        await Task.Delay(10, token);
                    }
                }
            }
            catch
            {
            }
        }

        private async Task<bool> SendOutputValuesAsync(int v0, int v1, int v2, int v3, CancellationToken token)
        {
            try
            {
                _sendBuffer[0] = (byte)((Math.Abs(v0) & 0xfe) | 0x02 | (v0 < 0 ? 1 : 0));
                _sendBuffer[1] = (byte)((Math.Abs(v1) & 0xfe) | 0x02 | (v1 < 0 ? 1 : 0));
                _sendBuffer[2] = (byte)((Math.Abs(v2) & 0xfe) | 0x02 | (v2 < 0 ? 1 : 0));
                _sendBuffer[3] = (byte)((Math.Abs(v3) & 0xfe) | 0x02 | (v3 < 0 ? 1 : 0));

                return await _bleDevice?.WriteAsync(_characteristic, _sendBuffer, token);
            }
            catch
            {
                return false;
            }
        }
    }
}
