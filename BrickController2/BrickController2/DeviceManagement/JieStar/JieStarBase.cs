using System;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement.JieStar;

/// <summary>
/// JIESTAR baseclass for devices with a nibble per channel
/// </summary>
internal abstract class JieStarBase : BluetoothAdvertisingDevice
{
    /// <summary>
    /// offset to position of first channel in base telegram
    /// </summary>
    private const int CHANNEL_START_OFFSET = 3;

    /// <summary>
    /// platform specific JIESTAR stuff
    /// </summary>
    protected readonly IJieStarPlatformService _jieStarPlatformService;

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
    /// The second context value for JieStar communication.
    /// </summary>
    private readonly byte _ctxValue2;

    /// <summary>
    /// array to hold the incoming output values for all channels.
    /// </summary>
    protected readonly float[] _storedValues;

    protected JieStarBase(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IJieStarPlatformService jieStarPlatformService, IJieStarDeviceManager jieStarDeviceManager, byte[] telegram_Connect, byte[] telegram_Base, byte ctxValue2)
        : base(name, address, deviceData, deviceRepository, bleService)
    {
        _telegram_Connect = telegram_Connect;
        _telegram_Base = telegram_Base;
        _ctxValue2 = ctxValue2;
        _jieStarPlatformService = jieStarPlatformService;
        _storedValues = new float[NumberOfChannels]; // initialize output values for all channels

        // bytes[1] and [2] of both telegrams can be set to a unique appId
        ReadOnlySpan<byte> appId = jieStarDeviceManager.GetAppId().Span[..2];
        _telegram_Connect[1] = appId[0];
        _telegram_Connect[2] = appId[1];

        _telegram_Base[1] = appId[0];
        _telegram_Base[2] = appId[1];
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
            bool valueChanged = SetChannelOutput(channelNo, value);

            // check for change
            if (valueChanged)
            {
                _bluetoothAdvertisingDeviceHandler.NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Processes the specified channel value and returns a transformed result.
    /// </summary>
    /// <remarks>The exact transformation logic and conditions for success are determined by the implementing
    /// class.</remarks>
    /// <param name="channelNo">The channel number to process. Must be a non-negative integer.</param>
    /// <param name="value">The input value associated with the channel to be processed.</param>
    /// <returns>A tuple containing the processed result: <list type="bullet"> <item> <description><c>value</c>: A byte
    /// representing the transformed value for the specified channel.</description> </item> <item>
    /// <description><c>flag</c>: A boolean indicating whether the value is marked as zero (<see langword="true"/>) or
    /// not (<see langword="false"/>).</description> </item> </list></returns>
    protected abstract (byte value, bool flag) ProcessChannelValue(int channelNo, float value);

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
    /// Converts a floating-point value into a nibble representation for an analog channel output.
    /// </summary>
    /// <remarks>The method maps the input value to a nibble representation based on predefined ranges for
    /// positive, negative, and zero values. The zero value is represented by a specific nibble constant. The caller can
    /// use the returned boolean to determine if the nibble corresponds to the zero value.</remarks>
    /// <param name="value">The floating-point value to be converted. Negative values are mapped to the negative range, positive values are
    /// mapped to the positive range, and zero is mapped to a predefined nibble.</param>
    /// <returns>A tuple containing the following: <list type="bullet"> <item> <description> A <see cref="byte"/> representing
    /// the nibble value for the analog channel output. </description> </item> <item> <description> A <see cref="bool"/>
    /// indicating whether the nibble corresponds to the zero value. <see langword="true"/> if the nibble represents
    /// zero; otherwise, <see langword="false"/>. </description> </item> </list></returns>
    protected (byte setValue_Nibble, bool zeroSet) SetOutput_AnalogChannel(float value)
    {
        // MK4: ZEROVALUE_NIBBLE = 0x08, RANGE_POS_OFFSET = 0x08
        // value <  0:  7 6 5 4 3 2 1                    RANGE_NEG: 0x07
        // value == 0:                0 8
        // value >  0:                    9 A B C D E F  RANGE_POS: 0x07

        const byte RANGE_POS_OFFSET = 0x08;
        const int RANGE_POS = 0x07;
        const int RANGE_NEG = 0x07;

        const float MIN_NEG_RANGE_THRESHOLD = -1f / RANGE_NEG;    // Minimum value for negative range
        const float MIN_POS_RANGE_THRESHOLD = 1f / RANGE_POS;     // Minimum value for positive range

        const byte ZEROVALUE_NIBBLE = 0x00;

        if (value <= MIN_NEG_RANGE_THRESHOLD)
        {
            float value_abs = Math.Min(0x07, -value * RANGE_NEG);
            byte setValue_nibble = (byte)(0x0F & (byte)value_abs);

            return (setValue_nibble, false);
        }
        else if (value >= MIN_POS_RANGE_THRESHOLD)
        {
            float value_abs = Math.Min(0x0F, (value * RANGE_POS) + RANGE_POS_OFFSET);
            byte setValue_nibble = (byte)(0x0F & (byte)(value_abs));

            return (setValue_nibble, false);
        }
        else
        {
            return (ZEROVALUE_NIBBLE, true);
        }
    }

    /// <summary>
    /// Sets the output value for the specified channel.
    /// </summary>
    /// <remarks>This method handles both virtual and real channels. For virtual channels, the value is
    /// processed and the modification status is returned. For real channels, the method calculates the appropriate
    /// byte offset and channel-specific parameters, processes the value, and updates  the channel state
    /// accordingly.</remarks>
    /// <param name="channelNo">The channel number for which the output value is to be set. Must be a valid channel identifier.</param>
    /// <param name="value">The output value to set for the specified channel. The value is processed before being applied.</param>
    /// <returns><see langword="true"/> if the channel's output value was modified; otherwise, <see langword="false"/>.</returns>
    protected bool SetChannelOutput(int channelNo, float value)
    {
        // real channel
        (int byteOffset, bool isLowerNibble) = GetTargetPosition(channelNo);
        (byte setValue_nibble, bool zeroSet) = ProcessChannelValue(channelNo, value);

        _bluetoothAdvertisingDeviceHandler.SetChannelState(channelNo, zeroSet); // set global channel state
        return SetChannelValue(byteOffset, isLowerNibble, setValue_nibble);
    }

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
            SetChannelOutput(channelNo, zeroValue); // set all channels to zero using the channel specific function
        }
    }

    /// <summary>
    /// Disconnects the device and resets the output state of all channels to zero.
    /// </summary>
    /// <remarks>This method ensures that all channels are set to a zero output state during the disconnection
    /// process. It is intended to be called as part of the device's disconnection workflow.</remarks>
    protected override void DisconnectDevice()
    {
        const float zeroValue = 0.0f;

        for (int channelNo = 0; channelNo < NumberOfChannels; channelNo++)
        {
            // call _bluetoothAdvertisingDeviceHandler.SetChannelState() to set global channel state to zero
            SetChannelOutput(channelNo, zeroValue);
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
            return _jieStarPlatformService.TryGetRfPayload(_ctxValue2, _telegram_Connect, out payload);
        }
        else
        {
            return _jieStarPlatformService.TryGetRfPayload(_ctxValue2, _telegram_Base, out payload);
        }
    }

    /// <summary>
    /// Calculates the target position of a channel within the current instance.
    /// </summary>
    /// <remarks>The calculation takes into account the instance number and assumes that each instance
    /// contains a fixed number of bytes for channels. Channels are packed two per byte, with the lower nibble
    /// representing one channel and the upper nibble representing the other.</remarks>
    /// <param name="channelNo">The channel number for which the position is calculated. Must be a non-negative integer.</param>
    /// <returns>A tuple containing the byte offset and a boolean indicating whether the target position is in the lower nibble.
    /// <list type="bullet"> <item><description><c>byteOffset</c>: The byte offset within the data structure where the
    /// channel is located.</description></item> <item><description><c>isLowerNibble</c>: <see langword="true"/> if the
    /// channel is in the lower nibble of the byte; otherwise, <see langword="false"/>.</description></item> </list></returns>
    protected virtual (int byteOffset, bool isLowerNibble) GetTargetPosition(int channelNo)
    {
        return (
            CHANNEL_START_OFFSET + (channelNo >> 1), // div 2 -> 2 channels per byte
            (channelNo & 0x01) == 0x01
        );
    }
}
