using BrickController2.PlatformServices.Permission;
using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;

namespace BrickController2.Windows.PlatformServices.Permission;

/// <summary>
/// Permission that is always granted.
/// Used for unpackaged Windows apps: they have no AppxManifest.xml, so there are no
/// capability declarations to check and MAUI's permission check would fail.
/// </summary>
public class GrantedPermission : IReadWriteExternalStoragePermission, IBluetoothPermission, ICameraPermission
{
    public Task<PermissionStatus> CheckStatusAsync() => Task.FromResult(PermissionStatus.Granted);

    public Task<PermissionStatus> RequestAsync() => Task.FromResult(PermissionStatus.Granted);
}
