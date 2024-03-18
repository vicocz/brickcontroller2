using Android.Content;
using Android.Content.PM;
using BrickController2.PlatformServices.Versioning;

namespace BrickController2.Tizen.PlatformServices.Versioning;

public class VersionService : IVersionService
{
    public string ApplicationVersion
    {
        get
        {
            try
            {
                return PackageInfo.VersionName;
            }
            catch (PackageManager.NameNotFoundException)
            {
                return "Unkonwn version";
            }
        }
    }

    private PackageInfo PackageInfo => _context.PackageManager.GetPackageInfo(_context.PackageName, 0);
}