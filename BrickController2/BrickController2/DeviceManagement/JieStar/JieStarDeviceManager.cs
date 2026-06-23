using System;
using BrickController2.UI.Services.AppIdentifier;

namespace BrickController2.DeviceManagement.JieStar;

/// <summary>
/// Manager for JIESTAR devices
/// </summary>
public class JieStarDeviceManager
{
    private const int AppIdentifierLength = 2; // JIESTAR protocol defines 2 bytes for the app identifier

    private readonly ReadOnlyMemory<byte> _appIdentifier;

    public JieStarDeviceManager(IAppIdentifierService appIdentifierService)
    {
        _appIdentifier = appIdentifierService.GetAppId(AppIdentifierLength);
    }

    public ReadOnlyMemory<byte> GetAppId() => _appIdentifier;
}