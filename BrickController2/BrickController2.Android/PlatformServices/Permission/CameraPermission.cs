using BrickController2.PlatformServices.Permission;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace BrickController2.Droid.PlatformServices.Permission
{
    internal class CameraPermission : BasePlatformPermission, ICameraPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new (string androidPermission, bool isRuntime)[]
        {
            (Android.Manifest.Permission.Camera, true)
        };
    }
}