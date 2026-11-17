using BrickController2.PlatformServices.SharedFileStorage;
using System;
using System.IO;
using Windows.Storage;

namespace BrickController2.Windows.PlatformServices.SharedFileStorage;

public class SharedFileStorageService : ISharedFileStorageService
{
    private readonly string? _sharedStorageBaseDirectory;

    public SharedFileStorageService(string? sharedStorageDirectory)
    {
        _sharedStorageBaseDirectory = sharedStorageDirectory;
    }

    public bool IsSharedStorageAvailable => _sharedStorageBaseDirectory != null;

    public bool IsPermissionGranted { get; set; }

    public string? SharedStorageBaseDirectory => _sharedStorageBaseDirectory;

    public string? SharedStorageDirectory => _sharedStorageBaseDirectory;

    public static string? GetPackagedSharedStorageDirectory()
    {
        try
        {
            return ApplicationData.Current.RoamingFolder.Path;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static string? GetUnpackagedSharedStorageDirectory()
    {
        try
        {
            // return i.e. C:\\Users\\me\\AppData\\Roaming\\BrickController2
            string bc2StorageDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ISharedFileStorageService.SharedDirectoryName);

            if (!Directory.Exists(bc2StorageDirectory))
            {
                Directory.CreateDirectory(bc2StorageDirectory);
            }

            return bc2StorageDirectory;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
