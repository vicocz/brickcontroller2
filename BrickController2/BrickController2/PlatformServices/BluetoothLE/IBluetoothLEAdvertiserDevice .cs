using System;
using System.Threading.Tasks;

namespace BrickController2.PlatformServices.BluetoothLE
{
    /// <summary>
    /// Enumeration for AdvertisingInterval
    /// Min is the fastest advertising rate.
    /// </summary>
    public enum AdvertisingInterval
    {
        /// <summary>
        /// Minimum value for advertising interval.
        /// Fastest advertising rate.
        /// </summary>
        Min,

        /// <summary>
        /// Perform high frequency, low latency advertising, around every 100ms.
        /// </summary>
        Low,

        /// <summary>
        /// Advertise on medium frequency, around every 250ms.
        /// </summary>
        Medium,

        /// <summary>
        /// Advertise on low frequency, around every 1000ms.
        /// </summary>
        High,

        /// <summary>
        /// Maximum value for advertising interval.
        /// Slowest advertising rate.
        /// </summary>
        Max,
    }

    /// <summary>
    /// Enumeration for PowerLevel
    /// </summary>
    public enum TxPowerLevel
    {
        Min,
        UltraLow,
        Low,
        Medium,
        High,
        Max,
    }

    public interface IBluetoothLEAdvertiserDevice :
        IDisposable
    {
        Task StartAdvertiseAsync(AdvertisingInterval advertisingIterval, TxPowerLevel txPowerLevel, ushort manufacturerId, byte[] rawData);

        Task StopAdvertiseAsync();

        Task UpdateAdvertisedDataAsync(ushort manufacturerId, byte[] rawData);
    }
}
