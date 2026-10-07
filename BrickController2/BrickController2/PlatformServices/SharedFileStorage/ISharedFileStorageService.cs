namespace BrickController2.PlatformServices.SharedFileStorage
{
    public interface ISharedFileStorageService
    {
        public const string SharedDirectoryName = "BrickController2";

        bool IsSharedStorageAvailable { get; }

        bool IsPermissionGranted { get; set; }

        string? SharedStorageBaseDirectory { get; }

        string? SharedStorageDirectory { get; }
    }
}
