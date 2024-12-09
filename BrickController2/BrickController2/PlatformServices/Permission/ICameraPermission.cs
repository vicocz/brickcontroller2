using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;

namespace BrickController2.PlatformServices.Permission;

public interface ICameraPermission
{
    Task<PermissionStatus> CheckStatusAsync();
    Task<PermissionStatus> RequestAsync();
}
