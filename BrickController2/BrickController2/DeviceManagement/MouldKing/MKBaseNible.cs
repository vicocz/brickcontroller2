using System;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// MK baseclass for devices with a nibble per channel
    /// </summary>
    internal abstract class MKBaseNible : BluetoothAdvertisingDevice
    {
        /// <summary>
        /// platform specific MouldKing stuff
        /// </summary>
        protected readonly IMKPlatformService _mkPlatformService;

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

        protected MKBaseNible(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, int channelStartOffset, byte[] telegram_Connect, byte[] telegram_Base, IMKPlatformService mkPlatformService)
            : base(name, address, deviceData, deviceRepository, bleService)
        {
            _channelStartOffset = channelStartOffset;
            _telegram_Connect = telegram_Connect;
            _telegram_Base = telegram_Base;
            _mkPlatformService = mkPlatformService;
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
                        // notify data changed
                        _bluetoothAdvertisingDeviceHandler.NotifyDataChanged(CheckAllChannelsZero());
                    }
                    else
                    {
                        // notify data changed
                        _bluetoothAdvertisingDeviceHandler.NotifyDataChanged(false);
                    }
                }
            }
        }

        /// <summary>
        /// This method sets the device to initial state before advertising starts
        /// </summary>
        protected override void InitDevice()
        {
            // set all channels to zero
            for (int index = 0; index < BaseTelegram_ChannelBytesCount; index++)
            {
                _telegram_Base[BaseTelegram_ChannelStartOffset + index] = 0x88;
            }
        }

        protected bool TryGetTelegram(bool getConnectTelegram, out byte[] payload)
        {
            if (getConnectTelegram)
            {
                return _mkPlatformService.TryGetRfPayload(_telegram_Connect, out payload);
            }
            else
            {
                return _mkPlatformService.TryGetRfPayload(_telegram_Base, out payload);
            }
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
