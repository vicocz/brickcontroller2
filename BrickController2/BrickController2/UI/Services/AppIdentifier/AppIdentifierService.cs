using System;
using BrickController2.UI.Services.Preferences;

namespace BrickController2.UI.Services.AppIdentifier
{
    public class AppIdentifierService : IAppIdentifierService
    {
        private const int MIN_APPID_LENGTH = 3; // minimum length of the AppIdentifier in bytes, to be valid
        private const string SECTION = "App";
        private const string APPIDKEY = "Identifier";

        private readonly object _lock = new object();
        private readonly IPreferencesService _preferencesService;

        // AppIdentifier is a byte-array that is used to identify the app on the device and to create a unique pairing between app and device.
        // It is patched into the advertising data of the device and is used to identify the app on the device-scan.
        // It is also used to create a unique pairing between app and device.
        // The AppIdentifier is stored in the preferences and is persistent across app restarts and updates.
        // It is created on first run and can be reset by clearing the preferences.
        private byte[] _appIdentifier;

        public AppIdentifierService(IPreferencesService preferencesService)
        {
            _preferencesService = preferencesService;
            _appIdentifier = GetPersistentAppIdentifier(_preferencesService, MIN_APPID_LENGTH);
        }

        public ReadOnlyMemory<byte> GetAppId(int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

            lock (_lock)
            {
                if (length > _appIdentifier.Length)
                {
                    // if the requested length is bigger than the current AppIdentifier, create a new one with the requested length,
                    // to keep the AppIdentifier as stable as possible
                    _appIdentifier = GetPersistentAppIdentifier(_preferencesService, length);
                }

                return new ReadOnlyMemory<byte>(_appIdentifier, 0, length);
            }
        }

        /// <summary>
        /// Gets or creates an app-persistent AppIdentifier.
        /// </summary>
        /// <param name="preferencesService">Reference to preferencesService singleton.</param>
        /// <param name="minLength">Minimum length of the AppIdentifier.</param>
        /// <returns>byte array containing the AppIdentifier</returns>
        private static byte[] GetPersistentAppIdentifier(IPreferencesService preferencesService, int minLength)
        {
            byte[]? appIdentifier = null;
            // gets or creates an app-persistent AppIdentifier
            try
            {
                if (preferencesService.ContainsKey(APPIDKEY, SECTION))
                {
                    // throws exception if converting went wrong
                    appIdentifier = Convert.FromBase64String(preferencesService.Get(APPIDKEY, string.Empty, SECTION));

                    // check minimum length
                    if (appIdentifier?.Length >= minLength)
                    {
                        return appIdentifier; // valid
                    }
                }
            }
            catch // catch all exceptions
            {
            }

            // create new byte[] with random values
            // * on first run
            // * on exception
            // * on length too short
            byte[] newAppIdentifier = new byte[minLength];
            Random.Shared.NextBytes(newAppIdentifier);

            if (appIdentifier != null)
            {
                // copy old values to new array, if possible, to keep the AppIdentifier as stable as possible
                Array.Copy(appIdentifier, newAppIdentifier, Math.Min(appIdentifier.Length, newAppIdentifier.Length));
            }

            try
            {
                // store new AppIdentifier in preferences, to be persistent across app restarts and updates
                preferencesService.Set(APPIDKEY, Convert.ToBase64String(newAppIdentifier), SECTION);
            }
            catch // catch all exceptions to keep app alive
            {
            }

            return newAppIdentifier;
        }
    }
}
