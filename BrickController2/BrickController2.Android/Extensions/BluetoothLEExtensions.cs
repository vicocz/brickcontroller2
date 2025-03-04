using System;
using Android.Bluetooth.LE;
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
        switch (advertisingInterval)
        {
            case AdvertisingInterval.Min:
                return AdvertisingSetParameters.IntervalMin;
            case AdvertisingInterval.Low:
                return AdvertisingSetParameters.IntervalLow;
            case AdvertisingInterval.Medium:
                return AdvertisingSetParameters.IntervalMedium;
            case AdvertisingInterval.High:
                return AdvertisingSetParameters.IntervalHigh;
            case AdvertisingInterval.Max:
                return AdvertisingSetParameters.IntervalMax;
            default:
                throw new ArgumentException("Illegal Argument", nameof(advertisingInterval));
        }
    }

    /// <summary>
    /// Converts BC2 enum TxPowerLevel to Android AdvertisingSetParameters enum AdvertiseTxPower
    /// </summary>
    /// <param name="txPowerLevel">enum TxPowerLevel</param>
    /// <returns>Android AdvertisingSetParameters enum AdvertiseTxPower</returns>
    public static AdvertiseTxPower ToTxPowerLevel(this TxPowerLevel txPowerLevel)
    {
        switch (txPowerLevel)
        {
            case TxPowerLevel.Min:
                return AdvertiseTxPower.Min;
            case TxPowerLevel.UltraLow:
                return AdvertiseTxPower.UltraLow;
            case TxPowerLevel.Low:
                return AdvertiseTxPower.Low;
            case TxPowerLevel.Medium:
                return AdvertiseTxPower.Medium;
            case TxPowerLevel.High:
                return AdvertiseTxPower.High;
            case TxPowerLevel.Max:
                return AdvertiseTxPower.Max;
            default:
                throw new ArgumentException("Illegal Argument", nameof(txPowerLevel));
        }
    }
}
