using BrickController2.CreationManagement;
using BrickController2.DeviceManagement.Sensor;
using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal abstract class ControlPlusDevice : BluetoothDevice
    {
        private static readonly Guid SERVICE_UUID = new Guid("00001623-1212-efde-1623-785feabcd123");
        private static readonly Guid CHARACTERISTIC_UUID = new Guid("00001624-1212-efde-1623-785feabcd123");

        private static readonly TimeSpan SEND_DELAY = TimeSpan.FromMilliseconds(40);

        /// Hub Property Message <see href="https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#hub-property-message-format"/>
        // Allow Button Report - 0x02 Button / 0x02 Enable Updates
        protected static readonly byte[] _activateButtonReportsMessage = new byte[] { 0x01, 0x02, 0x02 }.ToMessageTemplate();
        // Get Firmware version - 0x03 FW Version / 0x05 Request Update
        protected static readonly byte[] _requestFirmwareMessage = new byte[] { 0x01, 0x03, 0x05 }.ToMessageTemplate();
        // Allow batery updates - 0x06 	Battery Voltage / 0x02 Enable Updates
        protected static readonly byte[] _batteryLevelReportsMessage = new byte[] { 0x01, 0x06, 0x02 }.ToMessageTemplate();

        protected static readonly byte[] _voltageReportsMessage = new byte[] { 0x41, 0x3c, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01 }.ToMessageTemplate();
        protected static readonly byte[] _currentReportsMessage = new byte[] { 0x41, 0x3b, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01 }.ToMessageTemplate();

        private readonly byte[] _sendBuffer = new byte[] { 8, 0x00, 0x81, 0x00, 0x11, 0x51, 0x00, 0x00 };
        private readonly byte[] _servoSendBuffer = new byte[] { 14, 0x00, 0x81, 0x00, 0x11, 0x0d, 0x00, 0x00, 0x00, 0x00, 50, 50, 126, 0x00 };
        private readonly byte[] _virtualPortSendBuffer = new byte[] { 8, 0x00, 0x81, 0x00, 0x00, 0x02, 0x00, 0x00 };

        private readonly int[] _outputValues;
        private readonly int[] _lastOutputValues;
        private readonly int[] _maxServoAngles;
        private readonly int[] _servoBaseAngles;
        private readonly int[] _absolutePositions;
        private readonly int[] _relativePositions;

        private float _lastTiltX = 0;
        private float _lastTiltY = 0;
        private float _lastTiltZ = 0;

        protected float _current = 0;
        protected float _rssi = -100;

        private IGattCharacteristic _characteristic;

        public ControlPlusDevice(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : this(name, address, deviceRepository, bleService, new DevicePortInfo[0])
        {
        }

        public ControlPlusDevice(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IEnumerable<DevicePortInfo> ports)
            : base(name, address, deviceRepository, bleService)
        {
            _outputValues = new int[NumberOfChannels];
            _lastOutputValues = new int[NumberOfChannels];
            _maxServoAngles = new int[NumberOfChannels];
            _servoBaseAngles = new int[NumberOfChannels];
            _absolutePositions = new int[NumberOfChannels];
            _relativePositions = new int[NumberOfChannels];
        }
        protected override bool AutoConnectOnFirstConnect => true;

        public async override Task<DeviceConnectionResult> ConnectAsync(
            bool reconnect,
            Action<Device> onDeviceDisconnected,
            IEnumerable<ChannelConfiguration> channelConfigurations,
            bool startOutputProcessing,
            CancellationToken token)
        {
            for (int c = 0; c < NumberOfChannels; c++)
            {
                _outputValues[c] = 0;
                _lastOutputValues[c] = 0;
                _maxServoAngles[c] = -1;
                _servoBaseAngles[c] = 0;
                _absolutePositions[c] = 0;
                _relativePositions[c] = 0;
            }

            foreach (var channelConfig in channelConfigurations)
            {
                if (channelConfig.ChannelOutputType == ChannelOutputType.ServoMotor)
                {
                    _maxServoAngles[channelConfig.Channel] = channelConfig.MaxServoAngle;
                    _servoBaseAngles[channelConfig.Channel] = channelConfig.ServoBaseAngle;
                }
            }

            return await base.ConnectAsync(reconnect, onDeviceDisconnected, channelConfigurations, startOutputProcessing, token);
        }

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (int)(100 * value);
            if (_outputValues[channel] == intValue)
            {
                return;
            }

            _outputValues[channel] = intValue;
        }

        public override bool CanResetOutput => true;

        public override async Task ResetOutputAsync(int channel, float value, CancellationToken token)
        {
            CheckChannel(channel);

            await SetupChannelForPortInformationAsync(channel, token);
            await Task.Delay(300, token);
            await ResetServoAsync(channel, Convert.ToInt32(value * 180), token);
        }

        public override bool CanAutoCalibrateOutput => true;

        public override async Task<(bool Success, float BaseServoAngle)> AutoCalibrateOutputAsync(int channel, CancellationToken token)
        {
            CheckChannel(channel);

            await SetupChannelForPortInformationAsync(channel, token);
            await Task.Delay(300, token);

            return await AutoCalibrateServoAsync(channel, token);
        }

        protected override async Task<bool> ValidateServicesAsync(IEnumerable<IGattService> services, CancellationToken token)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
            _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID);

            if (_characteristic != null)
            {
                return await _bleDevice.EnableNotificationAsync(_characteristic, token);
            }

            return false;
        }

        protected override void OnCharacteristicChanged(Guid characteristicGuid, byte[] data)
        {
            if (characteristicGuid != CHARACTERISTIC_UUID || data.Length < 4)
            {
                return;
            }

            var messageType = data[2];
            switch (messageType)
            {
                case 0x01:
                    {
                        ProcessHubPropertyMessage(data);
                        return;
                    }
                case 0x04:
                    {
                        //TODO Allow full async
                        ProcessHubAttachedIoMessageAsync(data).Wait();
                        return;
                    }
                case 0x05:
                    {
                        ProcessErrorMessage(data);
                        return;
                    }
                case 0x43: // Port Information
                    {
                        //_parsePortInformationResponse(message);
                        return;
                    }
                case 0x45:
                    {
                        ProcessSensorMessage(data);
                        return;
                    }
                case 0x46: // Port value information
                    {
                        ProcessPortValueInformation(data);
                        return;
                    }
                case 0x47: // Port Input Format(Single)
                    {
                        return;
                    }
                case 0x82: // Port Output Command Feedback
                    {
                        //TODO
                        return;
                    }
            }

            System.Diagnostics.Debug.WriteLine($"MessageType: 0x{messageType:X} NOT SUPPORTED YET");
        }

        private void ParseMessageAsync(byte[] message)
        {
            if (message.Length <= 0)
            {
                return;
            }

            switch (message[2])
            {
                case 0x43:
                    {
                        //_parsePortInformationResponse(message);
                        break;
                    }
                case 0x44:
                    {
                        //                   this._parseModeInformationResponse(message);
                        break;
                    }
                case 0x82:
                    {
                        //                  this._parsePortAction(message);
                        break;
                    }
            }
        }

        private void ProcessPortValueInformation(byte[] data)
        {
            if (data.Length < 4)
            {
                return;
            }

            var portId = data[3];

            if (portId < 0 || portId >= NumberOfChannels)
            {
                return;
            }

            var modeMask = data[5];
            var dataIndex = 6;

            if ((modeMask & 0x01) != 0)
            {
                var absPosBuffer = BitConverter.IsLittleEndian ?
                    new byte[] { data[dataIndex + 0], data[dataIndex + 1] } :
                    new byte[] { data[dataIndex + 1], data[dataIndex + 0] };

                var absPosition = BitConverter.ToInt16(absPosBuffer, 0);
                _absolutePositions[portId] = absPosition;

                dataIndex += 2;
            }

            if ((modeMask & 0x02) != 0)
            {
                var relPosBuffer = BitConverter.IsLittleEndian ?
                    new byte[] { data[dataIndex + 0], data[dataIndex + 1], data[dataIndex + 2], data[dataIndex + 3] } :
                    new byte[] { data[dataIndex + 3], data[dataIndex + 2], data[dataIndex + 1], data[dataIndex + 0] };

                var relPosition = BitConverter.ToInt32(relPosBuffer, 0);
                _relativePositions[portId] = relPosition;
            }
        }

        private bool ProcessHubPropertyMessage(byte[] message)
        {
            // 3.5.2.Hub Property Message Format
            var propertyRef = message[3];

            if (propertyRef == 0x02)
            {
                // Button press reports
                var isPressed = (message[5] == 1);
                //TODO process

                return true;
            }
            if (propertyRef == 0x03)
            {
                // Firmware version
                var build = readUInt16LE(message, 5);
                var bugFix = message[7];
                var major = message[8] >> 4;
                var minor = message[8] & 0xf;
                FirmwareVersion = new Version(major, minor, bugFix, build);

                System.Diagnostics.Debug.WriteLine($"[MessageType:HubProperty, FirmWare={FirmwareVersion}");
                return true;
            }
            if (propertyRef == 0x06)
            {
                // Battery level reports
                BatteryLevel = message[5];

                //DEBUG System.Diagnostics.Debug.WriteLine($"[MessageType:HubProperty, BatteryLevel={BatteryLevel}");

                return true;
            }

            // DEBUG logging
            System.Diagnostics.Debug.WriteLine($"[MessageType:HubProperty, PropertyRef: 0x{propertyRef:X}");
            return false;
        }

        private void ProcessErrorMessage(byte[] data)
        {
            // Message Type - Error Messages [0x05]
            var commandId = data[3];
            var errorCode = data[4];

            switch (errorCode)
            {
                case 0x01:
                    System.Diagnostics.Debug.WriteLine($"MessageType: 0x05: ACK ERROR of 0x{commandId:X} command.");
                    break;
                case 0x02:
                    System.Diagnostics.Debug.WriteLine($"MessageType: 0x05: MACK ERROR of 0x{commandId:X} command.");
                    break;
                case 0x03:
                    System.Diagnostics.Debug.WriteLine($"MessageType: 0x05: Buffer Overflow ERROR of 0x{commandId:X} command.");
                    break;
                case 0x04:
                    System.Diagnostics.Debug.WriteLine($"MessageType: 0x05: Timeout ERROR of 0x{commandId:X} command.");
                    break;
                case 0x05:
                    System.Diagnostics.Debug.WriteLine($"MessageType: 0x05: Command NOT recognized ERROR of 0x{commandId:X} command.");
                    break;
                case 0x06:
                    System.Diagnostics.Debug.WriteLine($"MessageType: 0x05: Invalid use (e.g. parameter error(s) ERROR of 0x{commandId:X} command.");
                    break;
                case 0x07:
                    System.Diagnostics.Debug.WriteLine($"MessageType: 0x05: Overcurrent ERROR of 0x{commandId:X} command.");
                    break;
                case 0x08:
                    System.Diagnostics.Debug.WriteLine($"MessageType: 0x05: Internal ERROR of 0x{commandId:X} command.");
                    break;
            }
        }

        private static ushort readUInt16LE(byte[] data, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt16(data, startIndex);
            }

            return BitConverter.ToUInt16(new byte[] { data[startIndex + 1], data[startIndex] }, 0);
        }

        private void ProcessSensorMessage(byte[] message)
        {
            var portNumber = message[3];

            if (ProcessInternalSensorMessage(portNumber, message))
            {
                return;
            }

            if (!TryGetPort(portNumber, out var port) || !port.IsConnected)
            {
                // DEBUG logging
                System.Diagnostics.Debug.WriteLine($"[MessageType: SensorData: 0x{portNumber:X} UNKNOWN PORT");

                return;
            }

            if (ProcessSensorMessage(port, message))
            {

                return;
            }

            // DEBUG logging
            System.Diagnostics.Debug.WriteLine($"[MessageType: SensorData: {portNumber:X} NOT SUPPORTED YET.");
        }

        protected ushort ReadRawVoltage(byte[] sensorMessage)
        {
            return readUInt16LE(sensorMessage, 4);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual bool ProcessInternalSensorMessage(byte portNumber, byte[] message)
        {
            if (portNumber == 0x3c)
            {
                // Voltage
                var voltageRaw = ReadRawVoltage(message);
                Voltage = 9.600f * voltageRaw / 3893.0f;

                // DEBUG logging
                //System.Diagnostics.Debug.WriteLine($"[MessageType:Internal Sensor: {portNumber:X} Voltage: {Voltage}");

                return true;
            }
            if (portNumber == 0x3b)
            {
                // Current
                var currentRaw = ReadRawVoltage(message);
                _current = 2444f * currentRaw / 4095.0f;

                // DEBUG logging
                //System.Diagnostics.Debug.WriteLine($"[MessageType:Internal Sensor: {portNumber:X} Voltage: {Current}");

                return true;
            }
            return false;
        }

        protected virtual bool ProcessSensorMessage(DevicePort port, byte[] message)
        {
            if (port.PortType == DevicePortType.BOOST_DISTANCE)
            {
                var colorData = message[4];

                if (colorData <= 10)
                {
                    System.Diagnostics.Debug.WriteLine($"[MessageType:SensorData, Port:{port.Channel}, Color:{(BoostSensorColors)colorData}");
                }

                var distance = message[5];
                var partial = message[7];

                double distanceValue = (partial <= 0)
                    ? distance
                    : distance + 1.0 / partial;

                distanceValue = Math.Floor(distanceValue * 25.4) - 20;
                System.Diagnostics.Debug.WriteLine($"[MessageType:SensorData, Port:{port.Channel}, Distance:{distanceValue}");

                return true;
            }

            return false;
        }

        private async Task<bool> ProcessHubAttachedIoMessageAsync(byte[] data)
        {
            // 3.8.1. Hub Attached I/O Message Format
            byte portNumber = data[3];
            byte eventType = data[4];

            if (eventType == 0x00)
            {
                if (TryGetPort(portNumber, out var port))
                {
                    // when an attached motor or sensor is detached from the Hub
                    port.SetDisconnected();

                    // remove port if virtual
                    if (portNumber > 0x0f)
                        RemovePort(portNumber);
                }
            }
            else
            {
                // Unique type identification of the attached I/O device / 	UInt16
                DevicePortType type = (DevicePortType)readUInt16LE(data, 5);

                if (eventType == 0x02)
                {
                    // Event = Attached Virtual I/O
                    var portNumberA = data[7];
                    var portNumberB = data[8];
                    if (TryGetPort(portNumberA, out var portA) &&
                        TryGetPort(portNumberB, out var portB))
                    {
                        var newPortName = $"{portA.Name}{portB.Name}";
                        await RegisterNewPort(portNumber, newPortName, type);

                        System.Diagnostics.Debug.WriteLine($"Registered virtual port: 0x{portNumber:X}, Type:{type}");
                    }
                }
                else if (eventType == 0x01)
                {
                    if (TryGetPort(portNumber, out var port))
                    {
                        await OnPortDeviceAttaching(port, type);
                        await RequestPortInfoAsync(port.Channel);

                        System.Diagnostics.Debug.WriteLine($"Connected port: 0x{portNumber:X}, Type:{type}");
                    }
                    else
                    {
                        var portName = Enum.GetName(typeof(DevicePortType), type) ?? type.ToString();
                        await RegisterNewPort(portNumber, portName, type);

                        System.Diagnostics.Debug.WriteLine($"Internal port: 0x{portNumber:X}, Type:{type}");
                    }
                }
            }

            return true;
        }

        private async Task RegisterNewPort(byte channel, string name, DevicePortType portType)
        {
            var port = new DevicePort(channel, name);
            RegisterPorts(new[] { port });

            await OnPortDeviceAttaching(port, portType);
        }

        private async Task<bool> RequestPortInfoAsync(byte port, byte informationType = 0x01)
        {
            // https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-information-request
            // Message Type - Port Information Request [0x21]
            var message = new byte[] { 0x21, port, informationType }.ToMessageTemplate();

            return await _bleDevice?.WriteAsync(_characteristic, message, CancellationToken.None);
        }

        private async Task OnPortDeviceAttaching(DevicePort port, DevicePortType type)
        {
            // a motor or sensor is attached to the Hub
            port.SetConnected(type);

            if (AutoConnectOnFirstConnect)
            {
                var mode = GetDefaultModeForDeviceType(type);
                await SetupPortInputFormatAsync(port.Channel, type, mode, true);
            }
        }

        private byte GetDefaultModeForDeviceType(DevicePortType type)
        {
            switch (type)
            {
                case DevicePortType.Motor:
                case DevicePortType.SystemTrainMotor:
                case DevicePortType.ExternalMotorWithTacho:
                case DevicePortType.InternalMotorWithTacho:
                case DevicePortType.CONTROL_PLUS_LARGE_MOTOR:
                case DevicePortType.CONTROL_PLUS_XLARGE_MOTOR:
                    return 0x02;
                case DevicePortType.BOOST_DISTANCE:
                    return 0x08;
                case DevicePortType.InternalTilt:
                    // 0x04: 3 axis (precise)
                    return 0x04;
                default:
                    return 0x00;
            }
        }

        private int GetDefaultDeltaIntervalDeviceType(DevicePortType type)
        {
            switch (type)
            {
                case DevicePortType.InternalTilt:
                    // 5 degress for 0x04 precise 3 axis
                    return 5;
                default:
                    return 1;
            }
        }

        protected async Task<bool> SetupPortInputFormatAsync(byte port, DevicePortType type, byte mode, bool isActive)
        {
            // Message Type - Port Input Format Setup (Single) [0x41]
            // https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-input-format-setup-single

            byte isActiveFlag = isActive ? (byte)0x01 : (byte)0x00;

            var message = new byte[] { 0x41, port, mode, 0x01, 0x00, 0x00, 0x00, isActiveFlag }.ToMessageTemplate();

            return await _bleDevice?.WriteAsync(_characteristic, message, CancellationToken.None);
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            try
            {
                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    _outputValues[channel] = 0;
                    _lastOutputValues[channel] = 1;
                }

                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    await SendOutputValuesAsync(token);
                    await Task.Delay(10, token);
                }
            }
            catch
            {
                // Do nothing here, just exit
            }
        }

        protected override async Task<bool> AfterConnectSetupAsync(CancellationToken token)
        {
            try
            {
                // setup required type of notifications
                await _bleDevice?.WriteAsync(_characteristic, _activateButtonReportsMessage, token);
                await _bleDevice?.WriteAsync(_characteristic, _requestFirmwareMessage, token);
                await _bleDevice?.WriteAsync(_characteristic, _batteryLevelReportsMessage, token);
                await _bleDevice?.WriteAsync(_characteristic, _voltageReportsMessage, token);
                await _bleDevice?.WriteAsync(_characteristic, _currentReportsMessage, token);

                // Wait until ports finish communicating with the hub
                await Task.Delay(1000, token);

                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    if (_maxServoAngles[channel] >= 0)
                    {
                        await SetupChannelForPortInformationAsync(channel, token);
                        await Task.Delay(300, token);
                        await ResetServoAsync(channel, _servoBaseAngles[channel], token);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendOutputValuesAsync(CancellationToken token)
        {
            try
            {
                var result = true;

                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    var outputValue = _outputValues[channel];
                    var maxServoAngle = _maxServoAngles[channel];

                    if (maxServoAngle < 0)
                    {
                        result = result && await SendOutputValueAsync(channel, outputValue, token);
                    }
                    else
                    {
                        result = result && await SendServoOutputValueAsync(channel, outputValue, maxServoAngle, token);
                    }
                }

                return result;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendOutputValueAsync(int channel, int value, CancellationToken token)
        {
            try
            {
                if (_lastOutputValues[channel] != value)
                {
                    _sendBuffer[3] = (byte)channel;
                    _sendBuffer[7] = (byte)(value < 0 ? (255 + value) : value);

                    if (await _bleDevice?.WriteNoResponseAsync(_characteristic, _sendBuffer, token))
                    {
                        _lastOutputValues[channel] = value;

                        await Task.Delay(SEND_DELAY, token);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendOutputValueVirtualAsync(int virtualChannel, int channel1, int channel2, int value1, int value2, CancellationToken token)
        {
            try
            {
                if (_lastOutputValues[channel1] != value1 || _lastOutputValues[channel2] != value2)
                {
                    _virtualPortSendBuffer[3] = (byte)virtualChannel;
                    _virtualPortSendBuffer[6] = (byte)(value1 < 0 ? (255 + value1) : value1);
                    _virtualPortSendBuffer[7] = (byte)(value2 < 0 ? (255 + value2) : value2);

                    if (await _bleDevice?.WriteNoResponseAsync(_characteristic, _virtualPortSendBuffer, token))
                    {
                        _lastOutputValues[channel1] = value1;
                        _lastOutputValues[channel2] = value2;

                        await Task.Delay(SEND_DELAY, token);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendServoOutputValueAsync(int channel, int value, int maxServoAngle, CancellationToken token)
        {
            try
            {
                if (_lastOutputValues[channel] != value)
                {
                    var servoValue = maxServoAngle * value / 100;
                    _servoSendBuffer[3] = (byte)channel;
                    _servoSendBuffer[6] = (byte)(servoValue & 0xff);
                    _servoSendBuffer[7] = (byte)((servoValue >> 8) & 0xff);
                    _servoSendBuffer[8] = (byte)((servoValue >> 16) & 0xff);
                    _servoSendBuffer[9] = (byte)((servoValue >> 24) & 0xff);
                    _servoSendBuffer[10] = CalculateServoSpeed(maxServoAngle * _lastOutputValues[channel] / 100, servoValue);

                    if (await _bleDevice?.WriteNoResponseAsync(_characteristic, _servoSendBuffer, token))
                    {
                        _lastOutputValues[channel] = value;

                        await Task.Delay(SEND_DELAY, token);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SetupChannelForPortInformationAsync(int channel, CancellationToken token)
        {
            try
            {
                var lockBuffer = new byte[] { 0x05, 0x00, 0x42, (byte)channel, 0x02 };
                var inputFormatForAbsAngleBuffer = new byte[] { 0x0a, 0x00, 0x41, (byte)channel, 0x03, 0x02, 0x00, 0x00, 0x00, 0x01 };
                var inputFormatForRelAngleBuffer = new byte[] { 0x0a, 0x00, 0x41, (byte)channel, 0x02, 0x02, 0x00, 0x00, 0x00, 0x01 };
                var modeAndDataSetBuffer = new byte[] { 0x08, 0x00, 0x42, (byte)channel, 0x01, 0x00, 0x30, 0x20 };
                var unlockAndEnableBuffer = new byte[] { 0x05, 0x00, 0x42, (byte)channel, 0x03 };

                var result = true;
                result = result && await _bleDevice?.WriteAsync(_characteristic, lockBuffer, token);
                await Task.Delay(20);
                result = result && await _bleDevice?.WriteAsync(_characteristic, inputFormatForAbsAngleBuffer, token);
                await Task.Delay(20);
                result = result && await _bleDevice?.WriteAsync(_characteristic, inputFormatForRelAngleBuffer, token);
                await Task.Delay(20);
                result = result && await _bleDevice?.WriteAsync(_characteristic, modeAndDataSetBuffer, token);
                await Task.Delay(20);
                result = result && await _bleDevice?.WriteAsync(_characteristic, unlockAndEnableBuffer, token);

                return result;
            }
            catch
            {
                return false;
            }
        }
        
        private async Task<bool> ResetServoAsync(int channel, int baseAngle, CancellationToken token)
        {
            try
            {
                baseAngle = Math.Max(-180, Math.Min(179, baseAngle));

                var resetToAngle = NormalizeAngle(_absolutePositions[channel] - baseAngle);

                var result = true;

                result = result && await Reset(channel, 0, token);
                result = result && await Stop(channel, token);
                result = result && await Turn(channel, 0, 40, token);
                await Task.Delay(50);
                result = result && await Stop(channel, token);
                result = result && await Reset(channel, resetToAngle, token);
                result = result && await Turn(channel, 0, 40, token);
                await Task.Delay(500);
                result = result && await Stop(channel, token);

                var diff = Math.Abs(NormalizeAngle(_absolutePositions[channel] - baseAngle));
                if (diff > 5)
                {
                    // Can't reset to base angle, rebease to current position not to stress the plastic
                    result = result && await Reset(channel, 0, token);
                    result = result && await Stop(channel, token);
                    result = result && await Turn(channel, 0, 40, token);
                    await Task.Delay(50);
                    result = result && await Stop(channel, token);
                }

                return result;
            }
            catch
            {
                return false;
            }
        }

        private async Task<(bool, float)> AutoCalibrateServoAsync(int channel, CancellationToken token)
        {
            try
            {
                var result = true;

                result = result && await Reset(channel, 0, token);
                result = result && await Stop(channel, token);
                result = result && await Turn(channel, 0, 50, token);
                await Task.Delay(600);
                result = result && await Stop(channel, token);
                await Task.Delay(500);
                var absPositionAt0 = _absolutePositions[channel];
                result = result && await Turn(channel, -160, 60, token);
                await Task.Delay(600);
                result = result && await Stop(channel, token);
                await Task.Delay(500);
                var absPositionAtMin160 = _absolutePositions[channel];
                result = result && await Turn(channel, 160, 60, token);
                await Task.Delay(600);
                result = result && await Stop(channel, token);
                await Task.Delay(500);
                var absPositionAt160 = _absolutePositions[channel];

                var midPoint1 = NormalizeAngle((absPositionAtMin160 + absPositionAt160) / 2);
                var midPoint2 = NormalizeAngle(midPoint1 + 180);

                var baseAngle = (Math.Abs(NormalizeAngle(midPoint1 - absPositionAt0)) < Math.Abs(NormalizeAngle(midPoint2 - absPositionAt0))) ?
                    RoundAngleToNearest90(midPoint1) :
                    RoundAngleToNearest90(midPoint2);
                var resetToAngle = NormalizeAngle(_absolutePositions[channel] - baseAngle);

                result = result && await Reset(channel, 0, token);
                result = result && await Stop(channel, token);
                result = result && await Turn(channel, 0, 40, token);
                await Task.Delay(50);
                result = result && await Stop(channel, token);
                result = result && await Reset(channel, resetToAngle, token);
                result = result && await Turn(channel, 0, 40, token);
                await Task.Delay(600);
                result = result && await Stop(channel, token);

                return (result, baseAngle / 180F);
            }
            catch
            {
                return (false, 0F);
            }
        }

        private int NormalizeAngle(int angle)
        {
            if (angle >= 180)
            {
                return angle - (360 * ((angle + 180) / 360));
            }
            else if (angle < -180)
            {
                return angle + (360 * ((180 - angle) / 360));
            }

            return angle;
        }

        private int RoundAngleToNearest90(int angle)
        {
            angle = NormalizeAngle(angle);
            if (angle < -135) return -180;
            if (angle < -45) return -90;
            if (angle < 45) return 0;
            if (angle < 135) return 90;
            return -180;
        }

        private byte CalculateServoSpeed(int currentAngle, int targetAngle)
        {
            var diff = Math.Abs(currentAngle - targetAngle);
            var result = (byte)Math.Max(40, Math.Min(100, diff * 3));
            return result;
        }

        private Task<bool> Stop(int channel, CancellationToken token)
        {
            return _bleDevice.WriteAsync(_characteristic, new byte[] { 0x08, 0x00, 0x81, (byte)channel, 0x11, 0x51, 0x00, 0x00 }, token);
        }

        private Task<bool> Turn(int channel, int angle, int speed, CancellationToken token)
        {
            angle = NormalizeAngle(angle);

            var a0 = (byte)(angle & 0xff);
            var a1 = (byte)((angle >> 8) & 0xff);
            var a2 = (byte)((angle >> 16) & 0xff);
            var a3 = (byte)((angle >> 24) & 0xff);

            return _bleDevice.WriteAsync(_characteristic, new byte[] { 0x0e, 0x00, 0x81, (byte)channel, 0x11, 0x0d, a0, a1, a2, a3, (byte)speed, 0x64, 0x7e, 0x00 }, token);
        }

        private Task<bool> Reset(int channel, int angle, CancellationToken token)
        {
            angle = NormalizeAngle(angle);

            var a0 = (byte)(angle & 0xff);
            var a1 = (byte)((angle >> 8) & 0xff);
            var a2 = (byte)((angle >> 16) & 0xff);
            var a3 = (byte)((angle >> 24) & 0xff);

            return _bleDevice.WriteAsync(_characteristic, new byte[] { 0x0b, 0x00, 0x81, (byte)channel, 0x11, 0x51, 0x02, a0, a1, a2, a3 }, token);
        }
    }
}
