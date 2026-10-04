using BrickController2.PlatformServices.Permission;
using System;
using System.Collections.Generic;

namespace BrickController2.Windows.PlatformServices.Permission;

public class BluetoothPermission : SavePlatformPermission, IBluetoothPermission
{
    protected override Func<IEnumerable<string>> RequiredDeclarations => () => ["bluetooth"];
}