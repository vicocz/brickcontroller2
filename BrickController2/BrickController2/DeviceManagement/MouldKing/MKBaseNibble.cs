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
    /// array to hold the incoming output values for all channels.
    /// </summary>
    protected readonly float[] _storedValues;

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
    /// Determines whether the specified channel number represents a virtual channel.
    /// </summary>
    /// <remarks>This method can be overridden in a derived class to provide custom logic for identifying
    /// virtual channels. By default, it always returns <see langword="false"/>.</remarks>
    /// <param name="channelNo">The channel number to evaluate.</param>
    /// <returns><see langword="true"/> if the specified channel number is a virtual channel; otherwise, <see langword="false"/>.</returns>
    protected virtual bool IsVirtualChannel(int channelNo) => false;

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
        if (IsVirtualChannel(channelNo))
        {
            // virtual channel
            (byte _, bool isModified) = ProcessChannelValue(channelNo, value);
            return isModified;
        }
        else
        {
            // real channel
            (int byteOffset, bool isLowerNibble) = GetTargetPosition(channelNo);
            int specificChannelNo = GetSpecificChannelNumber(channelNo);

            (byte setValue_nibble, bool zeroSet) = ProcessChannelValue(channelNo, value);

            _bluetoothAdvertisingDeviceHandler.SetChannelState(specificChannelNo, zeroSet); // set global channel state
            return SetChannelValue(byteOffset, isLowerNibble, setValue_nibble);
        }
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
            return _mkPlatformService.TryGetRfPayload(_telegram_Connect, out payload);
        }
        else
        {
            return _mkPlatformService.TryGetRfPayload(_telegram_Base, out payload);
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
        // i.e. MK4.0 has 3 instances, each with 2 bytes for channels
        // instance 0: 3..4
        // instance 1: 5..6
        // instance 2: 7..8

        return (
            CHANNEL_START_OFFSET + _instanceNo * MAX_CHANNEL_BYTES_PER_INSTANCE + (channelNo >> 1), // div 2 -> 2 channels per byte
            (channelNo & 0x01) == 0x01
        ); 
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
