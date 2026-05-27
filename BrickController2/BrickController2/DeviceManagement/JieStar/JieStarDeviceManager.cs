using System;
using BrickController2.UI.Services.Preferences;

namespace BrickController2.DeviceManagement.JieStar;

/// <summary>
/// Manager for JieStar devices
/// </summary>
public class JieStarDeviceManager : IJieStarDeviceManager
{
    private const string SECTION = "JieStar";
    private const string APPIDKEY = "AppID";


    // this identifier is patched into the advertising data to identify the app
    private readonly byte[] _appIdChecksumMaskArray;

    public JieStarDeviceManager(IPreferencesService preferencesService)
    {
        _appIdChecksumMaskArray = JieStarDeviceManager.GetAppIdentifier(preferencesService);
    }

    public ReadOnlyMemory<byte> GetAppId() => _appIdChecksumMaskArray;

    /// <summary>
    /// gets or creates an App-persistant AppIdentifier
    /// </summary>
    /// <param name="preferencesService">reference to preferencesService singleton</param>
    /// <returns>byte array containing the AppIdentifier</returns>
    private static byte[] GetAppIdentifier(IPreferencesService preferencesService)
    {
        byte[] appIdChecksumMaskArray;
        // gets or creates an App-persistant AppIdentifier
        try
        {
            if (preferencesService.ContainsKey(APPIDKEY, SECTION))
            {
                // throws exception if converting went wrong
                appIdChecksumMaskArray = Convert.FromBase64String(preferencesService.Get(APPIDKEY, string.Empty, SECTION));

                // check minimum length
                if (appIdChecksumMaskArray?.Length >= 3)
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
        // * on length to short
        appIdChecksumMaskArray = new byte[3];

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