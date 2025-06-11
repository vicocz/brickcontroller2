using System;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// MK baseclass for devices with a nibble per channel
/// </summary>
internal abstract class MKBaseNibble : BluetoothAdvertisingDevice
{
    /// <summary>
    /// Channel types
    /// </summary>
    protected enum ChannelType
    {
        /// <summary>
        /// channel is analog
        /// </summary>
        Analog,

        /// <summary>
        /// channel can be set to left, off or right
        /// </summary>
        Left_Off_Right
    }

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

    /// <summary>
    /// combined low and high nibble of zeroValue
    /// </summary>
    protected readonly byte _zeroValueByte;

    protected MKBaseNibble(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IMKPlatformService mkPlatformService, int channelStartOffset, byte[] telegram_Connect, byte[] telegram_Base)
        : base(name, address, deviceData, deviceRepository, bleService)
    {
        _channelStartOffset = channelStartOffset;
        _telegram_Connect = telegram_Connect;
        _telegram_Base = telegram_Base;
        _mkPlatformService = mkPlatformService;

        _zeroValueByte = (byte)((ZeroValueNibble << 4) + ZeroValueNibble); // combined low and high nibble of zeroValue
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

    /// <summary>
    /// Gets the nibble value that represents zero in the current encoding scheme.
    /// MK3.8 has 0x09, MK4.0 has 0x08, MK5.0 has 0x00
    /// </summary>
    protected abstract byte ZeroValueNibble { get; }

    /// <summary>
    /// Gets the offset for positive values
    /// MK3.8 has 0x09, MK4.0 has 0x08, MK5.0 has 0x08
    /// </summary>
    protected abstract byte Range_pos_Offset { get; }

    /// <summary>
    /// Gets the range for positive values
    /// MK3.8 has 0x06, MK4 has 0x07, MK5 has 0x07
    /// </summary>
    protected abstract int Range_pos { get; }

    /// <summary>
    /// Gets the range for negative values
    /// MK3.8 has 0x07, MK4 has 0x07, MK5 has 0x07
    /// </summary>
    protected abstract int Range_neg { get; }

    /// <summary>
    /// Sets the output value for the specified channel.
    /// </summary>
    /// <remarks>This method adjusts the output value based on the channel type and applies the appropriate
    /// encoding. If the output value changes, a notification is triggered to indicate the change. For channels where
    /// the value is set to zero, additional checks are performed to determine if all channels are zero.</remarks>
    /// <param name="channelNo">The channel number for which the output value is to be set. Must be a valid channel index.</param>
    /// <param name="value">The output value to set. The value is interpreted based on the channel type: negative values, zero, and positive
    /// values may have different effects depending on the channel configuration.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the channel type is unknown or unsupported.</exception>
    public override void SetOutput(int channelNo, float value)
    {
        CheckChannel(channelNo);
        value = CutOutputValue(value);

        int channelOffset;
        ChannelType channelType;
        SelectChannel(channelNo, out channelOffset, out channelType);

        byte originValue_byte = _telegram_Base[channelOffset];

        byte setValue_byte;
        bool zeroSet;

        switch (channelType)
        {
            case ChannelType.Analog:
                // MK4: ZeroValueNibble = 0x08, ZeroValueOffset = 0x08
                // value <  0:  7 6 5 4 3 2 1                    range_neg: 0x07
                // value == 0:                0 8
                // value >  0:                    9 A B C D E F  range_pos: 0x07

                // MK3.8: ZeroValueNibble = 0x09, ZeroValueOffset = 0x09
                // value <  0:  7 6 5 4 3 2 1                    range_neg: 0x07
                // value == 0:                0 9
                // value >  0:                    A B C D E F    range_pos: 0x06

                // MK5: ZeroValueNibble = 0x00, ZeroValueOffset = 0x08
                // value <  0:  7 6 5 4 3 2 1                    range_neg: 0x07
                // value == 0:                0
                // value >  0:                  9 A B C D E F    range_pos: 0x07

                byte setValue_nibble;
                if (value < 0)
                {
                    float value_abs = Math.Min(0x07, -value * Range_neg);
                    setValue_nibble = (byte)(0x0F & (byte)value_abs);

                    if (setValue_nibble == 0) // replace zero with ZeroValueNibble
                    {
                        setValue_nibble = ZeroValueNibble;
                        zeroSet = true;
                    }
                    else
                    {
                        zeroSet = false;
                    }
                }
                else if (value > 0)
                {
                    float value_abs = Math.Min(0x0F, (value * Range_pos) + Range_pos_Offset);
                    setValue_nibble = (byte)(0x0F & (byte)(value_abs));
                    zeroSet = false;
                }
                else
                {
                    setValue_nibble = ZeroValueNibble;
                    zeroSet = true;
                }

                bool isOdd = (channelNo & 0x01) == 0x01; // is odd
                if (isOdd)
                {
                    setValue_byte = (byte)((originValue_byte & 0xF0) + setValue_nibble);
                }
                else
                {
                    setValue_byte = (byte)((originValue_byte & 0x0F) + (setValue_nibble << 4));
                }

                break;
            case ChannelType.Left_Off_Right:

                if (value < 0)
                {
                    setValue_byte = 1;
                    zeroSet = false;
                }
                else if (value > 0)
                {
                    setValue_byte = 2;
                    zeroSet = false;
                }
                else
                {
                    setValue_byte = 0;
                    zeroSet = true;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(channelType), $"Unknown channel type: {channelType}.");
        }


        lock (_outputLock)
        {
            // check for change
            if (_telegram_Base[channelOffset] != setValue_byte)
            {
                _telegram_Base[channelOffset] = setValue_byte;

                // Zero was set -> check all channel's values
                if (zeroSet)
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
    /// All channels are initialized with zeroValue.
    /// </summary>
    protected override void InitDevice()
    {
        // set all channels to zero
        for (int index = 0; index < BaseTelegram_ChannelBytesCount; index++)
        {
            _telegram_Base[BaseTelegram_ChannelStartOffset + index] = _zeroValueByte;
        }
    }

    /// <summary>
    /// Selects a channel based on the specified channel number and provides its offset and type.
    /// </summary>
    /// <remarks>The method calculates the channel offset based on the provided channel number and assigns a
    /// default channel type. Override this method in a derived class to customize channel selection behavior.</remarks>
    /// <param name="channelNo">The number of the channel to select. Must be a non-negative integer.</param>
    /// <param name="channelOffset">When this method returns, contains the calculated offset for the selected channel.</param>
    /// <param name="channelType">When this method returns, contains the type of the selected channel.</param>
    protected virtual void SelectChannel(int channelNo, out int channelOffset, out ChannelType channelType)
    {
        channelOffset = _channelStartOffset + (channelNo >> 1); // div 2
        channelType = ChannelType.Analog;
    }

    /// <summary>
    /// Attempts to retrieve the RF payload for the specified telegram type.
    /// </summary>
    /// <remarks>This method delegates the retrieval of the RF payload to the underlying platform
    /// service.</remarks>
    /// <param name="getConnectTelegram">A boolean value indicating the type of telegram to retrieve.  <see langword="true"/> to retrieve the connect
    /// telegram; <see langword="false"/> to retrieve the base telegram.</param>
    /// <param name="payload">When this method returns, contains the RF payload as a byte array if the operation succeeds; otherwise, <see
    /// langword="null"/>.</param>
    /// <returns><see langword="true"/> if the RF payload was successfully retrieved; otherwise, <see langword="false"/>.</returns>
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

    /// <summary>
    /// check all channels for zeroValue
    /// </summary>
    /// <returns>True if all channel equals zeroValue</returns>
    protected virtual bool CheckAllChannelsZero()
    {
        for (int index = 0; index < BaseTelegram_ChannelBytesCount; index++)
        {
            if (_telegram_Base[BaseTelegram_ChannelStartOffset + index] != _zeroValueByte)
            {
                return false;
            }
        }

        return true;
    }
}
