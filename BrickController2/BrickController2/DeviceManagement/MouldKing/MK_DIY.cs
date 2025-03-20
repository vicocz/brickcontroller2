using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private readonly byte[] _lastOutputValues;
        private readonly object _outputLock = new object();

        /// <summary>
        /// This buffer contains the needed values and channel's OutputValues (from offset 4 to 8) and a cross sum byte
        /// </summary>
        private readonly byte[] _sendOutputBuffer = { 0xcc, 0xaa, 0xbb, 0x01,                           // header
                                                      0x80, 0x80, 0x80, 0x80,                           // channel's values
                                                      0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80,   // ?
                                                      0x00,                                             // cross sum byte
                                                      0x33 };                                           // footer

        private volatile int _sendAttemptsLeft;

        private IGattCharacteristic? _characteristic_AE3B_CMD;

        public MK_DIY(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
            _lastOutputValues = new byte[NumberOfChannels];
        }

        public override DeviceType DeviceType => DeviceType.MK_DIY;

        public override int NumberOfChannels => 4;

        protected override bool AutoConnectOnFirstConnect => false;

        public override void SetOutput(int channelNo, float value)
        {
            CheckChannel(channelNo);
            value = CutOutputValue(value);
            int byteOffset = CHANNEL_START_OFFSET + channelNo;

            byte byteValue;
            if (value > 0)
            {
                float value_abs = Math.Min(0x7F, value * 0x7F);
                byteValue = (byte)(0x80 + value_abs);
            }
            else if (value < 0)
            {
                float value_abs = Math.Min(0x80, -value * 0x80);
                byteValue = (byte)(0x80 - value_abs);
            }
            else // if (intValue == 0)
            {
                byteValue = 0x80; // Zero
            }

            lock (_outputLock)
            {
                if (_sendOutputBuffer[byteOffset] != byteValue)
                {
                    _sendOutputBuffer[byteOffset] = byteValue;

                    ApplyCrossSum();

                    _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                }
            }
        }

        protected override Task<bool> ValidateServicesAsync(IEnumerable<IGattService>? services, CancellationToken token)
        {
            var service_AE3A = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID_AE3A_UNKNOWN_SERVICE);
            _characteristic_AE3B_CMD = service_AE3A?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID_AE3B_UNKNOWN_CHARACTERISTIC);

            return Task<bool>.FromResult(_characteristic_AE3B_CMD != null);
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            try
            {
                // on startup
                lock (_outputLock)
                {
                    for (int channelNo = 0; channelNo < NumberOfChannels; channelNo++)
                    {
                        _sendOutputBuffer[CHANNEL_START_OFFSET + channelNo] = 0x80;
                        _lastOutputValues[channelNo] = 0x00; // enshure that values are differnt
                    }
                    ApplyCrossSum();
                    _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                }

                byte[] localBuffer = new byte[_sendOutputBuffer.Length];
                Stopwatch lastSent = Stopwatch.StartNew();
                while (!token.IsCancellationRequested)
                {
                    int sendAttemptsLeft;

                    lock (_outputLock) // enshure consistency
                    {
                        Buffer.BlockCopy(_sendOutputBuffer, 0, localBuffer, 0, _sendOutputBuffer.Length);

                        sendAttemptsLeft = _sendAttemptsLeft;
                        _sendAttemptsLeft = sendAttemptsLeft > 0 ? sendAttemptsLeft - 1 : 0; // decrement _sendAttemptsLeft
                    }

                    bool sendOutputBufferHasChanged = false;
                    for (int channelNo = 0; channelNo < NumberOfChannels && !sendOutputBufferHasChanged; channelNo++)
                    {
                        sendOutputBufferHasChanged |= localBuffer[CHANNEL_START_OFFSET + channelNo] != _lastOutputValues[channelNo];
                    }

                    if (sendAttemptsLeft > 0 ||                 // sendAttemptsLeft
                        lastSent.Elapsed > ResendTimeSpan ||    // timeout
                        sendOutputBufferHasChanged)             // channel's values have changed
                    {
                        if (await SendOutputValuesAsync(localBuffer, token).ConfigureAwait(false))
                        {
                            Buffer.BlockCopy(localBuffer, CHANNEL_START_OFFSET, _lastOutputValues, 0, NumberOfChannels); // save channel's values last sent 
                            lastSent.Restart();
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

        private async Task<bool> SendOutputValuesAsync(byte[] sendOutputBuffer, CancellationToken token)
        {
            try
            {
                return await _bleDevice!.WriteNoResponseAsync(_characteristic_AE3B_CMD!, sendOutputBuffer, token);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ApplyCrossSum()
        {
            // array's byte at prelast position contains the cross sum of the array's bytes + 1
            int lastCalcIndex = _sendOutputBuffer.Length - 2;

            int sum = 0x01 + 0x33; // 0x01 is the startvalue, 0x33 is last byte in array
            for (int index = 0; index < lastCalcIndex; index++)
            {
                sum += _sendOutputBuffer[index];
            }
            _sendOutputBuffer[lastCalcIndex] = (byte)sum;
        }
    }
}
