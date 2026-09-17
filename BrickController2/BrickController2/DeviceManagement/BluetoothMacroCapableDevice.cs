using BrickController2.DeviceManagement.Macros;
using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement;

/// <summary>
/// Base class for Bluetooth devices that support dynamic macros
/// </summary>
internal abstract class BluetoothMacroCapableDevice : BluetoothDevice
{
    protected readonly AsyncLock _macroLock = new();
    private IReadOnlyList<MacroDescriptor> _availableMacros;

    public BluetoothMacroCapableDevice(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
        : base(name, address, deviceRepository, bleService)
    {
        _availableMacros = StaticMacros;
    }

    public sealed override bool SupportsMacros => true;
    public sealed override bool SupportsDynamicMacros => true;
    public sealed override IReadOnlyList<MacroDescriptor> AvailableMacros => Volatile.Read(ref _availableMacros);

    protected virtual IReadOnlyList<MacroDescriptor> StaticMacros => [];

    public sealed override async ValueTask<IReadOnlyList<MacroDescriptor>> GetMacrosAsync(bool forceRefresh = false, CancellationToken token = default)
    {
        if (!forceRefresh || !SupportsDynamicMacros || DeviceState != DeviceState.Connected)
        {
            return AvailableMacros;
        }

        try
        {
            using (await _macroLock.LockAsync(token))
            {
                if (DeviceState != DeviceState.Connected)
                {
                    return AvailableMacros;
                }

                var discoveredMacros = await DiscoverDynamicMacrosAsync(token);
                UpdateMacroCache(discoveredMacros);
            }
        }
        catch (OperationCanceledException)
        {
            // return cached results if the operation was canceled
        }

        return AvailableMacros;
    }

    protected abstract ValueTask<IReadOnlyList<MacroDescriptor>> DiscoverDynamicMacrosAsync(CancellationToken token);

    protected void UpdateMacroCache(IReadOnlyList<MacroDescriptor>? dynamicMacros)
    {
        dynamicMacros ??= [];

        _availableMacros = [.. StaticMacros, .. dynamicMacros];
        RaisePropertyChanged(nameof(AvailableMacros));
    }
}