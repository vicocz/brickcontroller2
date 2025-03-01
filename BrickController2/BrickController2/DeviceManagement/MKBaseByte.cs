using System;
using System.Diagnostics;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;

namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// MK baseclass for devices with one byte per channel
    /// </summary>
    internal abstract class MKBaseByte : BluetoothAdvertisingDevice
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

        protected MKBaseByte(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, int channelStartOffset, byte[] telegram_Connect, byte[] telegram_Base)
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
        /// offset to position of first channel in base telegram
        /// </summary>
        protected abstract int BaseTelegram_ChannelStartOffset { get; }

        /// <summary>
        /// number of bytes containing channel values in base telegram
        /// </summary>
        protected abstract int BaseTelegram_ChannelBytesCount { get; }

        /// <summary>
        /// This method sets the device to initial state before advertising starts
        /// </summary>
        protected override void InitDevice()
        {
            _isInitialized = false;
        }

        public override void SetOutput(int channelNo, float value)
        {
            CheckChannel(channelNo);
            value = CutOutputValue(value);
            byte byteValue;

            int byteOffset = _channelStartOffset + channelNo;

            if (value < 0)
            {
                float value_abs = Math.Min(0x80, -value * 0x80);
                byteValue = (byte)Math.Max(0x00, 0x80 - value_abs);
            }
            else if (value > 0)
            {
                float value_abs = Math.Min(0x80, value * 0x80);
                byteValue = (byte)Math.Min(0xFF, 0x80 + value_abs);
            }
            else
            {
                byteValue = 0x80;
            }

            lock (_outputLock)
            {
                // check for change
                if (_telegram_Base[byteOffset] != byteValue)
                {
                    _telegram_Base[byteOffset] = byteValue;

                    // Zero was set -> check all channel's values
                    if (byteValue == 0x80)
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

            MKProtocol.Get_rf_payload(MKProtocol.AddressArray, rawData, MKProtocol.CTXValue, out payload);
            return true;
        }

        private bool CheckAllChannelsZero()
        {
            for (int index = 0; index < BaseTelegram_ChannelBytesCount; index++)
            {
                if (_telegram_Base[BaseTelegram_ChannelStartOffset + index] != 0x80)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
