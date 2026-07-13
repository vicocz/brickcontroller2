using System;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.UI.Services.AppIdentifier;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// Manager for MouldKing devices
/// </summary>
public class MouldKingDeviceManager : BluetoothDeviceManagerBase,
    IMouldKingDeviceManager
{
    private const int AppIdentifierLength = 2; // MouldKing protocol defines 2 bytes for the app identifier

    private readonly ReadOnlyMemory<byte> _appIdentifier;

    public MouldKingDeviceManager(IAppIdentifierService appIdentifierService)
    {
        _appIdentifier = appIdentifierService.GetAppId(AppIdentifierLength);
    }

    public ReadOnlyMemory<byte> GetAppId() => _appIdentifier;

    protected override bool TryGetDeviceByManufacturerData(ScanResult scanResult,
        FoundDevice template,
        ushort manufacturerId,
        ReadOnlySpan<byte> manufacturerData,
        out FoundDevice device)
    {
        switch (manufacturerId)
        {
            case 0xac33:
                device = template with { DeviceType = DeviceType.MK_DIY };
                return true;
        }
        // no match
        device = default;
        return false;
    }
}
