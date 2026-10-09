using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.Windows.PlatformServices.Package;
using System;
using System.IO;
using Windows.Storage;

namespace BrickController2.Windows.PlatformServices.SharedFileStorage;

public class SharedFileStorageService : ISharedFileStorageService
{
    private readonly string? _sharedStorageBaseDirectory = InitialGetSharedStorageBaseDirectory();

    public bool IsSharedStorageAvailable => _sharedStorageBaseDirectory != null;

    public bool IsPermissionGranted { get; set; }

    public string? SharedStorageBaseDirectory => _sharedStorageBaseDirectory;

    public string? SharedStorageDirectory => _sharedStorageBaseDirectory;

    private static string? InitialGetSharedStorageBaseDirectory()
    {
        try
        {
            if (PackageHelper.IsPackaged)
            {
                // returns i.e. C:\Users\me\AppData\Local\Packages\com.scn.BrickController2_t9dkejnmt9jgw\RoamingState
                return ApplicationData.Current.RoamingFolder.Path; // throws an uncatchable exception if the app is not packaged
            }
            else
            {
                // return i.e. C:\\Users\\me\\AppData\\Roaming\\BrickController2
                string bc2StorageDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ISharedFileStorageService.SharedDirectoryName);

                if (!Directory.Exists(bc2StorageDirectory))
                {
                    Directory.CreateDirectory(bc2StorageDirectory);
                }

                return bc2StorageDirectory;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }
}
