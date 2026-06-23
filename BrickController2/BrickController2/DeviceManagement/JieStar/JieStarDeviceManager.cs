using System;
using BrickController2.UI.Services.Preferences;

namespace BrickController2.DeviceManagement.JieStar;

/// <summary>
/// Manager for JIESTAR devices
/// </summary>
public class JieStarDeviceManager : IJieStarDeviceManager
{
    private const string SECTION = "JIESTAR";
    private const string APPIDKEY = "AppID";


    // this identifier is patched into the advertising data to identify the app
    private readonly byte[] _appIdChecksumMaskArray;

    public JieStarDeviceManager(IPreferencesService preferencesService)
    {
        _appIdChecksumMaskArray = JieStarDeviceManager.GetAppIdentifier(preferencesService);
    }

    public ReadOnlyMemory<byte> GetAppId() => _appIdChecksumMaskArray;

    /// <summary>
    /// Gets or creates an app-persistent AppIdentifier.
    /// </summary>
    /// <param name="preferencesService">Reference to preferencesService singleton.</param>
    /// <returns>byte array containing the AppIdentifier</returns>
    private static byte[] GetAppIdentifier(IPreferencesService preferencesService)
    {
        byte[] appIdChecksumMaskArray;
        // gets or creates an app-persistent AppIdentifier
        try
        {
            if (preferencesService.ContainsKey(APPIDKEY, SECTION))
            {
                // throws exception if converting went wrong
                appIdChecksumMaskArray = Convert.FromBase64String(preferencesService.Get(APPIDKEY, string.Empty, SECTION));

                // check minimum length
                if (appIdChecksumMaskArray?.Length >= 2)
                {
                    return appIdChecksumMaskArray; // valid
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
        appIdChecksumMaskArray = new byte[2];

        Random.Shared.NextBytes(appIdChecksumMaskArray);

        try
        {
            preferencesService.Set(APPIDKEY, Convert.ToBase64String(appIdChecksumMaskArray), SECTION);
        }
        catch // catch all exceptions to keep app alive
        {
        }

        return appIdChecksumMaskArray;
    }
}