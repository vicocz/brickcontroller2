using BrickController2.PlatformServices.Permission;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace BrickController2.Tizen.PlatformServices.Permission;

public class ReadWriteExternalStoragePermission : BasePlatformPermission, IReadWriteExternalStoragePermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            // Let it be Granted (via empty permission list)
            Array.Empty<(string androidPermission, bool isRuntime)>();
}