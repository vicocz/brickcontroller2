using BrickController2.Helpers;
using BrickController2.PlatformServices.SharedFileStorage;
using System;
using System.IO;

namespace BrickController2.Tizen.PlatformServices.SharedFileStorage;

public class SharedFileStorageService : NotifyPropertyChangedSource, ISharedFileStorageService
{
    private static string _brickController2SharedDirectory = "BrickController2";

    public bool _isPermissionGranted = false;

    public bool IsSharedStorageAvailable => IsPermissionGranted && SharedStorageDirectory != null;

    public bool IsPermissionGranted
    {
        get { return _isPermissionGranted; }
        set
        {
            _isPermissionGranted = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(IsSharedStorageAvailable));
        }
    }

    public string? SharedStorageBaseDirectory
    {
        get
        {
            try
            {
                var storageDirectory = Environment.ExternalStorageDirectory?.AbsolutePath;

                if (storageDirectory == null || !Directory.Exists(storageDirectory) || !Environment.MediaMounted.Equals(storageState))
                {
                    return null;
                }

                return storageDirectory;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public string? SharedStorageDirectory
    {
        get
        {
            try
            {
                var storageDirectory = SharedStorageBaseDirectory;

                if (storageDirectory == null)
                {
                    return null;
                }

                var bc2StorageDirectory = Path.Combine(storageDirectory, _brickController2SharedDirectory);

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
}