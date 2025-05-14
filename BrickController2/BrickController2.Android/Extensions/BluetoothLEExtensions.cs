using System;
using Android.Bluetooth.LE;
using Android.Locations;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Droid.Extensions;

#pragma warning disable CA1416 // Validate platform compatibility

public static class BluetootLEExtensions
{
    /// <summary>
    /// Convert BC2 enum AdvertisingInterval to Android AdvertisingSetParameters Interval
    /// </summary>
    /// <param name="advertisingInterval">enum AdvertisingInterval</param>
    /// <returns>Android AdvertisingSetParameters Interval</returns>
    public static int ToInterval(this AdvertisingInterval advertisingInterval)
    {
        return advertisingInterval switch
        {
            AdvertisingInterval.Min => AdvertisingSetParameters.IntervalMin,
            AdvertisingInterval.Low => AdvertisingSetParameters.IntervalLow,
            AdvertisingInterval.Medium => AdvertisingSetParameters.IntervalMedium,
            AdvertisingInterval.High => AdvertisingSetParameters.IntervalHigh,
            AdvertisingInterval.Max => AdvertisingSetParameters.IntervalMax,
            _ => throw new ArgumentException("Illegal Argument", nameof(advertisingInterval))
        };
    }

    /// <summary>
    /// Converts BC2 enum TxPowerLevel to Android AdvertisingSetParameters enum AdvertiseTxPower
    /// </summary>
    /// <param name="txPowerLevel">enum TxPowerLevel</param>
    /// <returns>Android AdvertisingSetParameters enum AdvertiseTxPower</returns>
    public static AdvertiseTxPower ToTxPowerLevel(this TxPowerLevel txPowerLevel)
    {
        return txPowerLevel switch
        {
            TxPowerLevel.Min => AdvertiseTxPower.Min,
            TxPowerLevel.UltraLow => AdvertiseTxPower.UltraLow,
            TxPowerLevel.Low => AdvertiseTxPower.Low,
            TxPowerLevel.Medium => AdvertiseTxPower.Medium,
            TxPowerLevel.High => AdvertiseTxPower.High,
            TxPowerLevel.Max => AdvertiseTxPower.Max,
            _ => throw new ArgumentException("Illegal Argument", nameof(txPowerLevel))
        };
    }
}
