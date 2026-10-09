using BrickController2.PlatformServices.Permission;
using System;
using System.Collections.Generic;

namespace BrickController2.Windows.PlatformServices.Permission;

public class ReadWriteExternalStoragePermission : SafePlatformPermission, IReadWriteExternalStoragePermission
{
    protected override Func<IEnumerable<string>> RequiredDeclarations => () => ["removableStorage"];
}