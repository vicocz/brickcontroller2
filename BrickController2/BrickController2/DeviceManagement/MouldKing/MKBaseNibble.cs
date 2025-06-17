using System;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// MK baseclass for devices with a nibble per channel
/// </summary>
internal abstract class MKBaseNibble : BluetoothAdvertisingDevice
{
    /// <summary>
    /// offset to position of first channel in base telegram
    /// </summary>
    private const int CHANNEL_START_OFFSET = 3;

    /// <summary>
    /// number of maximal channels in the device type
    /// </summary>
    private const int MAX_CHANNEL_BYTES_PER_INSTANCE = 2;

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
    /// Represents an array of functions that process the channel's setvalue and return a tuple
    /// containing the baseTelegramChannelByteOffset, the channel specific setvalue and a boolean to indicate a zero value.
    /// </summary>
    protected readonly Func<float, (int, int, (byte, bool))>[] _setChannel;

    /// <summary>
    /// instance number of specific device type
    /// </summary>
    protected readonly int _instanceNo;

    protected MKBaseNibble(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IMKPlatformService mkPlatformService, int instanceNo, byte[] telegram_Connect, byte[] telegram_Base)
        : base(name, address, deviceData, deviceRepository, bleService)
    {
        _telegram_Connect = telegram_Connect;
        _telegram_Base = telegram_Base;
        _mkPlatformService = mkPlatformService;

        _instanceNo = instanceNo;

        _setChannel = CreateSetChannelList();
    }

    /// <summary>
    /// No voltage
    /// </summary>
    public override string BatteryVoltageSign => string.Empty;


    /// <summary>
    /// Sets the output value for the specified channel.
    /// </summary>
    /// <remarks>This method adjusts the output value based on the channel type and applies the appropriate
    /// encoding. If the output value changes, a notification is triggered to indicate the change. For channels where
    /// the value is set to zero, additional checks are performed to determine if all channels are zero.</remarks>
    /// <param name="channelNo">The channel number for which the output value is to be set. Must be a valid channel index.</param>
    /// <param name="value">The output value to set. The value is interpreted based on the channel type: negative values, zero, and positive
    /// values may have different effects depending on the channel configuration.</param>
    public override void SetOutput(int channelNo, float value)
    {
        CheckChannel(channelNo);
        value = CutOutputValue(value);

        (int byteOffset, int specificChannelNo, (byte setValue_nibble, bool zeroSet)) = _setChannel[channelNo](value);

        byte originValue_byte = _telegram_Base[byteOffset];

        bool isOdd = (channelNo & 0x01) == 0x01; // is odd
        byte setValue_byte;
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
            if (_telegram_Base[byteOffset] != setValue_byte)
            {
                _telegram_Base[byteOffset] = setValue_byte;

                // notify data changed
                _bluetoothAdvertisingDeviceHandler.NotifyDataChanged(specificChannelNo, zeroSet);
            }
        }
    }

    /// <summary>
    /// Creates and returns an array of functions, each corresponding to a channel
    /// </summary>
    /// <remarks>Each function in the returned array is specific to a channel and is generated using the <see
    /// cref="CreateSetChannel"/> method. The number of functions in the array matches the value of <see
    /// cref="NumberOfChannels"/>.</remarks>
    /// <returns>An array of functions</returns>
    protected Func<float, (int, int, (byte, bool))>[] CreateSetChannelList()
    {
        Func<float, (int, int, (byte, bool))>[] setChannelList = new Func<float, (int, int, (byte, bool))>[NumberOfChannels];

        for (int channelNo = 0; channelNo < NumberOfChannels; channelNo++)
        {
            int byteOffset = GetSpecificChannelByteOffset(channelNo);
            int specificChannelNo = GetSpecificChannelNumber(channelNo);
            Func<float, (byte, bool)> createSetChannel = CreateSetChannel(channelNo);

            setChannelList[channelNo] = (float value) => (byteOffset, specificChannelNo, createSetChannel(value));
        }
        return setChannelList;
    }


    /// <summary>
    /// Creates a delegate that sets the value of a specific channel and returns the result as a tuple.
    /// </summary>
    /// <remarks>The returned delegate allows dynamic configuration of the specified channel. The exact
    /// behavior and constraints of the delegate depend on the implementation in derived classes.</remarks>
    /// <param name="channelNo">The channel number to configure. Must be a valid channel identifier.</param>
    /// <returns>A function that takes a <see cref="float"/> value as input, representing the channel's desired state, and
    /// returns a tuple containing a <see cref="byte"/> representing the processed channel value and a <see
    /// langword="bool"/> indicating whether a zero value was set.</returns>
    protected abstract Func<float, (byte, bool)> CreateSetChannel(int channelNo);


    /// <summary>
    /// This method sets the device to initial state before advertising starts
    /// All channels are initialized with zeroValue.
    /// </summary>
    protected override void InitDevice()
    {
        for (int channelNo = 0; channelNo < NumberOfChannels; channelNo++)
        {
            _setChannel[channelNo](0); // set all channels to zero using the channel specific function
        }
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
    /// Calculates the byte offset inside the baseTelegram of the DeviceType for the specified channel.
    /// </summary>
    /// <param name="channelNo">The zero-based index of the channel for which to calculate the start offset.</param>
    /// <returns>The start offset for the specified channel.</returns>
    private int GetSpecificChannelByteOffset(int channelNo)
    {
        // instance 0: 3..4
        // instance 1: 5..6
        // instance 2: 7..8
        return CHANNEL_START_OFFSET + _instanceNo * MAX_CHANNEL_BYTES_PER_INSTANCE + (channelNo >> 1); // div 2, 2 channels per byte
    }

    /// <summary>
    /// Calculates the channel number inside the DeviceType for the specified channel.
    /// </summary>
    /// <param name="channelNo">The zero-based index of the channel for which to calculate the DeviceType specific channel number.</param>
    /// <returns>The DeviceType specific channel number for the specified channel.</returns>
    private int GetSpecificChannelNumber(int channelNo)
    {
        // instance 0: 0..3
        // instance 1: 4..7
        // instance 2: 8..11
        return _instanceNo * NumberOfChannels + channelNo;
    }
}
