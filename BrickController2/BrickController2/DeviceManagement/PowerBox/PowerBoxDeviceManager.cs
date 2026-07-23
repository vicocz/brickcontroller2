using System;
using BrickController2.UI.Services.AppIdentifier;

namespace BrickController2.DeviceManagement.PowerBox;

/// <summary>
/// Manager for PowerBox devices
/// </summary>
public class PowerBoxDeviceManager
{
    private const int AppIdentifierLength = 2; // PowerBox protocol defines 2 bytes for the app identifier

    private readonly ReadOnlyMemory<byte> _appIdentifier;

    public PowerBoxDeviceManager(IAppIdentifierService appIdentifierService)
    {
        _appIdentifier = appIdentifierService.GetAppId(AppIdentifierLength);
    }

    public ReadOnlyMemory<byte> GetAppId() => _appIdentifier;
}