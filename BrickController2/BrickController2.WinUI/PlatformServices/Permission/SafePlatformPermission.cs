using Microsoft.Maui.ApplicationModel;
using System.IO;
using System.Threading.Tasks;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace BrickController2.Windows.PlatformServices.Permission;

public abstract class SafePlatformPermission : BasePlatformPermission
{
    public override Task<PermissionStatus> CheckStatusAsync()
    {
        try
        {
            return base.CheckStatusAsync();
        }
        catch (FileNotFoundException)
        {
            // Unpackaged Windows apps have no AppxManifest.xml, so the MAUI
            // Permissions capability check (which tries to read it) always
            // throws here. There is no capability declaration mechanism for
            // unpackaged apps, so we treat this as implicitly granted - matches
            // MAUI's own intended behavior for unpackaged Windows apps.
        }
        return Task.FromResult(PermissionStatus.Granted);
    }
}