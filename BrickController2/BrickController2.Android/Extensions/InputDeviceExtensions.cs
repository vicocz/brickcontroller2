using Android.Views;

namespace BrickController2.Droid.Extensions;

public static class InputDeviceExtensions
{
    /// <summary>
    /// Get an unique persistant identifier string for the given inputdevice
    /// </summary>
    /// <param name="inputDevice">inputdevice</param>
    /// <returns>deviceidentifier</returns>
    public static string GetUniquePersistentDeviceId(this InputDevice? inputDevice) =>
        // https://developer.android.com/develop/ui/views/touch-and-input/game-controllers/multiple-controllers
        // Note: On devices running Android 4.1(API level 16) and higher, you can obtain an input device’s descriptor using getDescriptor(), which returns a unique persistent
        // string value for the input device.Unlike a device ID, the descriptor value won't change even if the input device is disconnected, reconnected, or reconfigured. 
        inputDevice?.Descriptor ?? "NoDescriptor";
}
