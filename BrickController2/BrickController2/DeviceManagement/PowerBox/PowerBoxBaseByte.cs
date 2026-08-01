using System;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement.PowerBox;

/// <summary>
/// PowerBox baseclass
/// </summary>
internal abstract class PowerBoxBaseByte : BluetoothAdvertisingDevice
{
    /// <summary>
    /// offset to position of first channel in base telegram
    /// </summary>
    private const int CHANNEL_START_OFFSET = 3;

    /// <summary>
    /// platform specific PowerBox stuff
    /// </summary>
    protected readonly IPowerBoxPlatformService _powerboxPlatformService;

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

    protected PowerBoxBaseByte(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IPowerBoxPlatformService powerboxPlatformService, PowerBoxDeviceManager powerboxDeviceManager, byte[] telegram_Connect, byte[] telegram_Base)
        : base(name, address, deviceData, deviceRepository, bleService)
    {
        _telegram_Connect = telegram_Connect;
        _telegram_Base = telegram_Base;
        _powerboxPlatformService = powerboxPlatformService;
        _storedValues = new float[NumberOfChannels]; // initialize output values for all channels

        // bytes[1] and [2] of both telegrams can be set to a unique appId
        ReadOnlySpan<byte> appId = powerboxDeviceManager.GetAppId().Span[..2];
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
    /// Updates a specific byte in the telegram buffer and returns whether the value was changed.
    /// </summary>
    /// <remarks>This method modifies the telegram buffer by updating the specified byte. The operation is 
    /// thread-safe and ensures exclusive access to the buffer during the
    /// update.</remarks>
    /// <param name="byteOffset">The zero-based index of the byte in the telegram buffer to modify.</param>
    /// <param name="setValue_byte">The value to set.param>
    /// <returns><see langword="true"/> if the byte in the telegram buffer was modified;  otherwise, <see langword="false"/> if
    /// the value remained unchanged.</returns>
    protected virtual bool SetChannelValue(int byteOffset, byte setValue_byte)
    {
        lock (_outputLock)
        {
            byte originValue_byte = _telegram_Base[byteOffset];

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
        // real channel
        int byteOffset = GetTargetPosition(channelNo);
        (byte setValue_byte, bool zeroSet) = ProcessChannelValue(channelNo, value);

        _bluetoothAdvertisingDeviceHandler.SetChannelState(channelNo, zeroSet); // set global channel state
        return SetChannelValue(byteOffset, setValue_byte);
    }

    /// <summary>
    /// This method sets the device to initial state before advertising starts
    /// All channels are initialized with zeroValue.
    /// </summary>
    protected override void InitDevice() => ResetAllChannelsToZero();

    /// <summary>
    /// Disconnects the device and resets the output state of all channels to zero.
    /// </summary>
    /// <remarks>This method ensures that all channels are set to a zero output state during the disconnection
    /// process. It is intended to be called as part of the device's disconnection workflow.</remarks>
    protected override void DisconnectDevice() => ResetAllChannelsToZero();

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
            return _powerboxPlatformService.TryGetRfPayload(_telegram_Connect, out payload);
        }
        else
        {
            return _powerboxPlatformService.TryGetRfPayload(_telegram_Base, out payload);
        }
    }

    /// <summary>
    /// Calculates the target position of a channel within the current instance.
    /// </summary>
    /// <param name="channelNo">The channel number for which the position is calculated. Must be a non-negative integer.</param>
    /// <returns>The byte offset.
    protected virtual int GetTargetPosition(int channelNo)
    {
        return CHANNEL_START_OFFSET + channelNo;
    }

    /// <summary>
    /// Resets the value and the output state of all channels to zero.
    /// </summary>
    private void ResetAllChannelsToZero()
    {
        const float zeroValue = 0.0f;

        for (int channelNo = 0; channelNo < NumberOfChannels; channelNo++)
        {
            _storedValues[channelNo] = zeroValue;
            SetChannelOutput(channelNo, zeroValue);
        }
    }
}
