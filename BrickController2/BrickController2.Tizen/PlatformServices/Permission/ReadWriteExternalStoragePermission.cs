using BrickController2.PlatformServices.Permission;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace BrickController2.Tizen.PlatformServices.Permission;

public class ReadWriteExternalStoragePermission : BasePlatformPermission, IReadWriteExternalStoragePermission
{
    public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
    [
        ("http://tizen.org/privilege/externalstorage.appdata", true),
    ];
}