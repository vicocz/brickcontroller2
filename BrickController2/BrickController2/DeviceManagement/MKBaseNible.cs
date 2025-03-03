using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using System;
using System.Diagnostics;

namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// MK baseclass for devices with a nibble per channel
    /// </summary>
    internal abstract class MKBaseNible : BluetoothAdvertisingDevice
    {
        /// <summary>
        /// stopwatch
        /// </summary>
        protected readonly Stopwatch _allZeroStopwatch = Stopwatch.StartNew();

        /// <summary>
        /// Telegram to connect to the device
        /// This telegram is sent on init and on reconnect conditions matching
        /// </summary>
        protected readonly byte[] _telegram_Connect;

        /// <summary>
        /// base telegram
        /// </summary>
        protected readonly byte[] _telegram_Base;

        /// <summary>
        /// byte offset to first channel in _telegram_base
        /// </summary>
        protected readonly int _channelStartOffset;

        /// <summary>
        /// true if initialized
        /// </summary>
        protected bool _isInitialized = false;

        /// <summary>
        /// true if all channel's values equal zero
        /// </summary>
        protected bool _allChannelsZero = true;

        /// <summary>
        /// after this timespan and all channel's values equal to zero the connect telegram is sent
        /// </summary>
        protected TimeSpan _reconnectTimeSpan = TimeSpan.FromSeconds(3);

        protected MKBaseNible(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, int channelStartOffset, byte[] telegram_Connect, byte[] telegram_Base)
            : base(name, address, deviceData, deviceRepository, bleService)
        {
            _channelStartOffset = channelStartOffset;
            _telegram_Connect = telegram_Connect;
            _telegram_Base = telegram_Base;
        }

        /// <summary>
        /// No voltage
        /// </summary>
        public override string BatteryVoltageSign => string.Empty;

        /// <summary>
        /// byte offset to position of first channel in base telegram
        /// </summary>
        protected abstract int BaseTelegram_ChannelStartOffset { get; }

        /// <summary>
        /// number of bytes containing channel values in base telegram
        /// </summary>
        protected abstract int BaseTelegram_ChannelBytesCount { get; }

        public override void SetOutput(int channelNo, float value)
        {
            CheckChannel(channelNo);
            value = CutOutputValue(value);

            bool isOdd = (channelNo & 0x01) == 0x01;                    // is odd
            int channelOffset = _channelStartOffset + (channelNo >> 1); // div 2
            byte originValue_byte = _telegram_Base[channelOffset];
            byte setValue_nibble;
            byte setValue_byte;


            if (value < 0)
            {
                float value_abs = Math.Min(0x07, -value * 0x07);
                setValue_nibble = (byte)(0x0F & (byte)value_abs);
            }
            else if (value > 0)
            {
                float value_abs = Math.Min(0x07, value * 0x07);
                setValue_nibble = (byte)(0x0F & (byte)(0x08 + value_abs));
            }
            else
            {
                setValue_nibble = 0x08;
            }

            if (isOdd)
            {
                setValue_byte = (byte)((originValue_byte & 0xF0) + setValue_nibble);
            }
            else
            {
                setValue_byte = (byte)((originValue_byte & 0x0F) + (setValue_nibble << 4));
            }

            lock (_outputLock)
            {
                // check for change
                if (_telegram_Base[channelOffset] != setValue_byte)
                {
                    _telegram_Base[channelOffset] = setValue_byte;

                    // Zero was set -> check all channel's values
                    if (setValue_nibble == 0x08)
                    {
                        _allChannelsZero = CheckAllChannelsZero();
                    }
                    else
                    {
                        _allChannelsZero = false;
                    }

                    // notify data changed
                    _bluetoothAdvertiser.NotifyDataChanged();
                }
            }
        }

        /// <summary>
        /// This method sets the device to initial state before advertising starts
        /// </summary>
        protected override void InitDevice()
        {
            _isInitialized = false;
        }

        protected bool TryGetTelegram(out byte[] payload)
        {
            byte[] rawData;

            if (!_isInitialized ||
                (_allChannelsZero && _allZeroStopwatch.Elapsed > _reconnectTimeSpan))
            {
                rawData = _telegram_Connect;

                _isInitialized = true;
            }
            else if (_allChannelsZero)
            {
                rawData = _telegram_Base;
            }
            else
            {
                rawData = _telegram_Base;
                _allZeroStopwatch.Restart();
            }

            MKProtocol.GetRfPayload(MKProtocol.AddressArray, rawData, MKProtocol.CTXValue, out payload);
            return true;
        }

        private bool CheckAllChannelsZero()
        {
            for (int index = 0; index < BaseTelegram_ChannelBytesCount; index++)
            {
                if (_telegram_Base[BaseTelegram_ChannelStartOffset + index] != 0x88)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
