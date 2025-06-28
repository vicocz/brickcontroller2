using BrickController2.PlatformServices.BluetoothLE;
using Newtonsoft.Json.Linq;
using System;

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
    /// Represents the virtual channel number used for specific operations.
    /// </summary>
    /// <remarks>This constant is intended for use in scenarios where a virtual channel identifier is
    /// required. The value is set to -1, which may indicate a special or default channel.</remarks>
    protected const int VIRTUALCHANNEL = -1; // virtual channel number

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
    /// array to hold the incoming output values for all channels.
    /// </summary>
    protected readonly float[] _storedValues;

    /// <summary>
    /// Represents an array of functions that process the channel's setvalue and return a tuple
    /// containing the baseTelegramChannelByteOffset, the channel specific setvalue and a boolean to indicate a zero value.
    /// </summary>
    protected readonly Func<float, bool>[] _setChannel;

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

        _storedValues = new float[NumberOfChannels]; // initialize output values for all channels
        _setChannel = CreateSetChannelList();
    }

    /// <summary>
    /// No voltage
    /// </summary>
    public override string BatteryVoltageSign => string.Empty;

    /// <summary>
    /// Sets the output value for the specified channel.
    /// </summary>
    /// <remarks>This method updates the output value for the specified channel and ensures the value is
    /// within the valid range. If the value changes, the method triggers a notification to indicate that data has been
    /// updated.</remarks>
    /// <param name="channelNo">The channel number for which the output value is being set. Must be a valid channel index.</param>
    /// <param name="value">The output value to set. The value will be adjusted if it exceeds the allowable range.</param>
    public override void SetOutput(int channelNo, float value)
    {
        CheckChannel(channelNo);
        value = CutOutputValue(value);

        // store the incoming value in the stored values array
        _storedValues[channelNo] = value;

        lock (_outputLock)
        {
            // call the channel specific set function
            bool valueChanged = _setChannel[channelNo](value);

            // check for change
            if (valueChanged)
            {
                _bluetoothAdvertisingDeviceHandler.NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Updates a specific nibble of a byte in the telegram buffer and returns whether the value was changed.
    /// </summary>
    /// <remarks>This method modifies the telegram buffer by updating either the lower or upper nibble of the
    /// specified byte. The operation is thread-safe and ensures exclusive access to the buffer during the
    /// update.</remarks>
    /// <param name="byteOffset">The zero-based index of the byte in the telegram buffer to modify.</param>
    /// <param name="isLowerNibble">A value indicating whether the lower nibble of the byte should be updated.  <see langword="true"/> to update the
    /// lower nibble; <see langword="false"/> to update the upper nibble.</param>
    /// <param name="setValue_nibble">The new nibble value to set, represented as a byte (0-15).</param>
    /// <returns><see langword="true"/> if the byte in the telegram buffer was modified;  otherwise, <see langword="false"/> if
    /// the value remained unchanged.</returns>
    protected bool SetChannelValue(int byteOffset, bool isLowerNibble, byte setValue_nibble)
    {
        lock (_outputLock)
        {
            byte originValue_byte = _telegram_Base[byteOffset];

            byte setValue_byte;
            if (isLowerNibble)
            {
                setValue_byte = (byte)((originValue_byte & 0xF0) + setValue_nibble);
            }
            else
            {
                setValue_byte = (byte)((originValue_byte & 0x0F) + (setValue_nibble << 4));
            }
            _telegram_Base[byteOffset] = setValue_byte;
            return _telegram_Base[byteOffset] != originValue_byte;
        }
    }

    /// <summary>
    /// Creates and initializes a list of channel-setting functions for all available channels.
    /// </summary>
    /// <remarks>Each function in the returned array is responsible for setting the state or value of a
    /// specific channel. The behavior of the function depends on whether the channel is virtual or real: - For virtual
    /// channels, the function determines whether the channel's state has been modified. - For real channels, the
    /// function updates the channel's state and value based on the provided input.</remarks>
    /// <returns>An array of functions, where each function takes a <see langword="float"/> value as input and returns a <see
    /// langword="bool"/> indicating success or modification. The array contains one function per channel, corresponding
    /// to the total number of channels.</returns>
    protected Func<float, bool>[] CreateSetChannelList()
    {
        Func<float, bool>[] setChannelList = new Func<float, bool>[NumberOfChannels];

        for (int channelNo = 0; channelNo < NumberOfChannels; channelNo++)
        {
            var (realChannelNo, handler) = CreateChannelHandler(channelNo);

            // virtual channel
            if (realChannelNo == VIRTUALCHANNEL)
            {
                setChannelList[channelNo] = (float value) =>
                {
                    // virtual channel handlers return isModified insted of zeroSet
                    (byte a, bool isModified) = handler(value);

                    return isModified;
                };
            }
            // real channel
            else
            {
                bool isOdd = (realChannelNo & 0x01) == 0x01;
                int byteOffset = GetByteOffset(realChannelNo);
                int specificChannelNo = GetSpecificChannelNumber(realChannelNo);

                setChannelList[channelNo] = (float value) =>
                {
                    (byte setValue_nibble, bool zeroSet) = handler(value);

                    _bluetoothAdvertisingDeviceHandler.SetChannelState(specificChannelNo, zeroSet); // set global channel state
                    return SetChannelValue(byteOffset, isOdd, setValue_nibble);
                };
            }
        }
        return setChannelList;
    }

    /// <summary>
    /// Creates a handler for processing a specific channel, returning the real channel number and a function to compute
    /// channel-specific values.
    /// </summary>
    /// <remarks>This method is abstract and must be implemented by derived classes to define the behavior for
    /// mapping and processing channel numbers.</remarks>
    /// <param name="channelNo">The logical channel number to be processed. Must be a valid channel number within the expected range.</param>
    /// <returns>A tuple containing: <list type="bullet"> <item> <description>The real channel number corresponding to the
    /// provided logical channel number or constant VIRTUALCHANNEL to mark as virtual.</description> </item> <item> <description>A function that takes a float input
    /// and returns a tuple consisting of a byte value (representing the nibble to set) and a boolean indicating whether
    /// the zero set condition is met.</description> </item> </list></returns>
    protected abstract (int realChannelNo, Func<float, (byte setValue_nibble, bool zeroSet)>) CreateChannelHandler(int channelNo);

    /// <summary>
    /// This method sets the device to initial state before advertising starts
    /// All channels are initialized with zeroValue.
    /// </summary>
    protected override void InitDevice()
    {
        const float zeroValue = 0.0f;

        for (int channelNo = 0; channelNo < NumberOfChannels; channelNo++)
        {
            _storedValues[channelNo] = zeroValue;   // restore stored values to zero
            _setChannel[channelNo](zeroValue);      // set all channels to zero using the channel specific function
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
    /// Calculates the byte offset for a given channel number within the current instance.
    /// </summary>
    /// <remarks>The byte offset is determined based on the instance number, the maximum number of bytes
    /// allocated per instance,  and the channel number. Each byte represents two channels.</remarks>
    /// <param name="channelNo">The channel number for which to calculate the byte offset. Must be a non-negative integer.</param>
    /// <returns>The byte offset corresponding to the specified channel number within the current instance.</returns>
    private int GetByteOffset(int channelNo)
    {
        // i.e. MK4.0 has 3 instances, each with 2 bytes for channels
        // instance 0: 3..4
        // instance 1: 5..6
        // instance 2: 7..8
        return CHANNEL_START_OFFSET + _instanceNo * MAX_CHANNEL_BYTES_PER_INSTANCE + (channelNo >> 1); // div 2 -> 2 channels per byte
    }

    /// <summary>
    /// Calculates the absolute channel number based on the specified relative channel number and the instance number.
    /// </summary>
    /// <param name="channelNo">The relative channel number within the current instance. Must be within the valid range for the instance.</param>
    /// <returns>The absolute channel number, combining the instance number and the relative channel number.</returns>
    private int GetSpecificChannelNumber(int channelNo)
    {
        // i.e. MK4.0 has 3 instances, each with 4 channels
        // instance 0:  0.. 3
        // instance 1:  4.. 7
        // instance 2:  8..11
        return _instanceNo * NumberOfChannels + channelNo;
    }
}
