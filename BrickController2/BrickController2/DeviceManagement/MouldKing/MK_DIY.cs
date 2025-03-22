using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// MouldKing DIY
    /// </summary>
    internal class MK_DIY : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 10;

        /// <summary>
        /// byte offset to first channel in _sendOutputBuffer
        /// </summary>
        private const int CHANNEL_START_OFFSET = 4;

        /// <summary>
        /// After this TimeSpan has elapsed since the last sending OutputValues are send again
        /// </summary>
        private static readonly TimeSpan ResendTimeSpan = TimeSpan.FromMilliseconds(100);

        private static readonly Guid SERVICE_UUID_AE3A_UNKNOWN_SERVICE = new Guid("0000ae3a-0000-1000-8000-00805f9b34fb");
        private static readonly Guid CHARACTERISTIC_UUID_AE3B_UNKNOWN_CHARACTERISTIC = new Guid("0000ae3b-0000-1000-8000-00805f9b34fb");

        private readonly int[] _lastOutputValues = new int[4];
        private readonly int[] _outputValues = new int[4];
        private readonly object _outputLock = new object();

        private int _sendAttemptsLeft;

        private IGattCharacteristic? _characteristic_AE3B_CMD;

        public MK_DIY(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.MK_DIY;

        public override int NumberOfChannels => 4;

        protected override bool AutoConnectOnFirstConnect => false;

        public override void SetOutput(int channelNo, float value)
        {
            CheckChannel(channelNo);
            value = CutOutputValue(value);

            var intValue = (int)(value * 0x80); // scale and cast

            lock (_outputLock)
            {
                if (_outputValues[channelNo] != intValue)
                {
                    _outputValues[channelNo] = intValue;
                    _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                }
            }
        }

        protected override Task<bool> ValidateServicesAsync(IEnumerable<IGattService>? services, CancellationToken token)
        {
            var service_AE3A = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID_AE3A_UNKNOWN_SERVICE);
            _characteristic_AE3B_CMD = service_AE3A?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_AE3B_UNKNOWN_CHARACTERISTIC);

            return Task.FromResult(_characteristic_AE3B_CMD != null);
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            try
            {
                lock (_outputLock)
                {
                    for (int channelNo = 0; channelNo < NumberOfChannels; channelNo++)
                    {
                        _outputValues[channelNo] = 0x00;
                        _lastOutputValues[channelNo] = 0x01;
                    }

                    _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                }

                int[] outputValues = new int[NumberOfChannels];
                int sendAttemptsLeft;

                while (!token.IsCancellationRequested)
                {
                    lock (_outputLock)
                    {
                        for (int channelNo = 0; channelNo < NumberOfChannels; channelNo++)
                        {
                            outputValues[channelNo] = _outputValues[channelNo];
                        }

                        sendAttemptsLeft = _sendAttemptsLeft;
                        _sendAttemptsLeft = sendAttemptsLeft > 0 ? sendAttemptsLeft - 1 : 0;
                    }

                    if (outputValues[0] != _lastOutputValues[0] || 
                        outputValues[1] != _lastOutputValues[1] || 
                        outputValues[2] != _lastOutputValues[2] || 
                        outputValues[3] != _lastOutputValues[3] || 
                        sendAttemptsLeft > 0)
                    {
                        if (await SendOutputValuesAsync(outputValues, token).ConfigureAwait(false))
                        {
                            for (int channelNo = 0; channelNo < NumberOfChannels; channelNo++)
                            {
                                _lastOutputValues[channelNo] = outputValues[channelNo];
                            }

                            // reset attemps due to success
                            lock (_outputLock)
                            {
                                _sendAttemptsLeft = 0;
                            }
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

        private async Task<bool> SendOutputValuesAsync(int[] outputValues, CancellationToken token)
        {
            // byte offset to first channel in _sendOutputBuffer
            const int CROSS_SUM_OFFSET = 16;

            // precalculated cross sum of the static values of the output buffer
            const int STATIC_CROSS_SUM =
                0x01 +                                                  // startoffset
                0xCC + 0xAA + 0xBB + 0x01 +                             // header
                //0x80 + 0x80 + 0x80 + 0x80 +                           // dynamic part: Channel 0 .. Channel 3
                0x80 + 0x80 + 0x80 + 0x80 + 0x80 + 0x80 + 0x80 + 0x80 + // ?
                //0x00                                                  // lower byte of cross sum
                0x33;                                                   // footer

            byte[] sendOutputBuffer = {
                0xCC, 0xAA, 0xBB, 0x01,                           // header
                0x80, 0x80, 0x80, 0x80,                           // channel's values
                0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,   // ?
                0x00,                                             // lower byte of cross sum
                0x33 };                                           // footer

            int crosssum = STATIC_CROSS_SUM;
            for (int channelNo = 0; channelNo < 4; channelNo++)
            {
                int intValue = outputValues[channelNo];

                byte byteValue = intValue switch
                {
                    > 0 => (byte)(0x80 + Math.Min(0x7F, intValue)),
                    < 0 => (byte)(0x80 - Math.Min(0x80, -intValue)),
                    _ => 0x80
                };

                sendOutputBuffer[CHANNEL_START_OFFSET + channelNo] = byteValue;
                crosssum += byteValue;
            }
            sendOutputBuffer[CROSS_SUM_OFFSET] = (byte)crosssum;

            try
            {
                return await _bleDevice!.WriteNoResponseAsync(_characteristic_AE3B_CMD!, sendOutputBuffer, token);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
