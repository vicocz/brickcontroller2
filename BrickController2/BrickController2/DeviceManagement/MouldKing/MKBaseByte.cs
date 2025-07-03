using System;
using BrickController2.PlatformServices.BluetoothLE;

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
    /// byte offset to first channel in _telegram_base
    /// </summary>
    protected readonly int _channelStartOffset;


    protected MKBaseByte(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, int channelStartOffset, byte[] telegram_Connect, byte[] telegram_Base, IMKPlatformService mkPlatformService)
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
    /// offset to position of first channel in base telegram
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

                _bluetoothAdvertisingDeviceHandler.SetChannelState(channelNo, byteValue == 0x80);
                _bluetoothAdvertisingDeviceHandler.NotifyDataChanged();
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
            _telegram_Base[BaseTelegram_ChannelStartOffset + index] = 0x80;
        }

        ResetAllChannelsToZero();
    }

    /// <summary>
    /// Disconnects the device and resets the state of all communication channels.
    /// </summary>
    /// <remarks>This method iterates through all available channels and sets their state to inactive.  It
    /// ensures that the device is properly disconnected and all channels are reset.</remarks>
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
            return _mkPlatformService.TryGetRfPayload(_telegram_Connect, out payload);
        }
        else
        {
            return _mkPlatformService.TryGetRfPayload(_telegram_Base, out payload);
        }
    }

    private void ResetAllChannelsToZero()
    {
        for (int channelNo = 0; channelNo < NumberOfChannels; channelNo++)
        {
            // call SetChannelState() to set global channel state to zero
            _bluetoothAdvertisingDeviceHandler.SetChannelState(channelNo, true);
        }
    }
}
