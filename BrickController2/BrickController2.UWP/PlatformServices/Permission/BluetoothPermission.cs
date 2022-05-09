using System.Collections.Generic;
using static Xamarin.Essentials.Permissions;
using BrickController2.PlatformServices.Permission;
using System;

namespace BrickController2.Windows.PlatformServices.Permission
{
    public class BluetoothPermission : BasePlatformPermission, IBluetoothPermission
    {
        protected override Func<IEnumerable<string>> RequiredDeclarations => () => new[] { "bluetooth" };
    }
}