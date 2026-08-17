using BrickController2.PlatformServices.BluetoothLE;
using System;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// MK baseclass for devices with one byte per channel
/// </summary>
internal abstract class MKBaseByte : BluetoothAdvertisingDevice
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
    /// array to hold the incoming output values for all channels.
    /// </summary>
    protected readonly float[] _storedValues;

    /// <summary>
    /// byte offset to first channel in _telegram_base
    /// </summary>
    protected readonly int _channelStartOffset;


    protected MKBaseByte(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, IMKPlatformService mkPlatformService, IMouldKingDeviceManager mkDeviceManager, int channelStartOffset, byte[] telegram_Connect, byte[] telegram_Base)
        : base(name, address, deviceData, deviceRepository, bleService)
    {
        _channelStartOffset = channelStartOffset;
        _telegram_Connect = telegram_Connect;
        _telegram_Base = telegram_Base;
        _mkPlatformService = mkPlatformService;

        // bytes[1] and [2] of both telegrams can be set to a unique appId
        ReadOnlySpan<byte> appId = mkDeviceManager.GetAppId().Span[..2];
        _telegram_Connect[1] = appId[0];
        _telegram_Connect[2] = appId[1];

        _telegram_Base[1] = appId[0];
        _telegram_Base[2] = appId[1];

        _storedValues = new float[NumberOfChannels]; // initialize output values for all channels

        // initialize all channels in _telegram_Base and _storedValues to zero value
        InitDevice();
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
    /// offset to position of first channel in base telegram
    /// </summary>
    protected abstract int BaseTelegram_ChannelStartOffset { get; }

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
            int byteOffset = GetTargetPosition(channelNo);

            (byte setValue_byte, bool zeroSet) = ProcessChannelValue(channelNo, value);

            _bluetoothAdvertisingDeviceHandler.SetChannelState(channelNo, zeroSet); // set global channel state
            return SetChannelValue(byteOffset, setValue_byte);
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
    /// <param name="setValue">The new nibble value to set, represented as a byte (0-15).</param>
    /// <returns><see langword="true"/> if the byte in the telegram buffer was modified;  otherwise, <see langword="false"/> if
    /// the value remained unchanged.</returns>
    protected bool SetChannelValue(int byteOffset, byte setValue)
    {
        lock (_outputLock)
        {
            byte originValue_byte = _telegram_Base[byteOffset];

            _telegram_Base[byteOffset] = setValue;

            return _telegram_Base[byteOffset] != originValue_byte;
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
    /// This method sets the device to initial state before advertising starts
    /// </summary>
    protected override void InitDevice() => ResetAllChannelsToZero();

    /// <summary>
    /// Disconnects the device and resets the state of all communication channels.
    /// </summary>
    /// <remarks>This method iterates through all available channels and sets their state to inactive.  It
    /// ensures that the device is properly disconnected and all channels are reset.</remarks>
    protected override void DisconnectDevice() => ResetAllChannelsToZero();

    /// <summary>
    /// Calculates the target position of a channel within the current instance.
    /// </summary>
    protected virtual int GetTargetPosition(int channelNo) => _channelStartOffset + channelNo;

    /// <summary>
    /// Converts a floating-point value into a byte representation for an analog channel output.
    /// </summary>
    /// <remarks>The method maps the input value to a byte representation based on predefined ranges for
    /// positive, negative, and zero values. The zero value is represented by a specific byte constant. The caller can
    /// use the returned boolean to determine if the byte corresponds to the zero value.</remarks>
    /// <param name="value">The floating-point value to be converted. Negative values are mapped to the negative range, positive values are
    /// mapped to the positive range, and zero is mapped to a predefined byte.</param>
    /// <returns>A tuple containing the following: <list type="bullet"> <item> <description> A <see cref="byte"/> representing
    /// the byte value for the analog channel output. </description> </item> <item> <description> A <see cref="bool"/>
    /// indicating whether the byte corresponds to the zero value. <see langword="true"/> if the byte represents
    /// zero; otherwise, <see langword="false"/>. </description> </item> </list></returns>
    protected (byte setValue_Byte, bool zeroSet) SetOutput_AnalogChannel(float value)
    {
        if (value < 0)
        {
            float value_abs = Math.Min(0x80, -value * 0x80);
            byte setValue_byte = (byte)Math.Max(0x00, 0x80 - value_abs);

            return (setValue_byte, false);
        }
        else if (value > 0)
        {
            float value_abs = Math.Min(0x80, value * 0x80);
            byte setValue_byte = (byte)Math.Min(0xFF, 0x80 + value_abs);
            return (setValue_byte, false);
        }
        else
        {
            byte setValue_byte = 0x80;
            return (setValue_byte, false);
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
    protected internal bool TryGetTelegram(bool getConnectTelegram, out byte[] payload)
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
    /// Resets the value and the output state of all channels to zero.
    /// </summary>
    protected void ResetAllChannelsToZero()
    {
        const float zeroValue = 0.0f;

        for (int channelNo = 0; channelNo < NumberOfChannels; channelNo++)
        {
            _storedValues[channelNo] = zeroValue;
            SetChannelOutput(channelNo, zeroValue);
        }
    }
}
