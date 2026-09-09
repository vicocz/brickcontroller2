using BrickController2.DeviceManagement.Macros;
using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement;

internal abstract class BluetoothDeviceWithMacros : BluetoothDevice
{
    private IReadOnlyList<MacroDescriptor> _availableMacros;

    public BluetoothDeviceWithMacros(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
        : base(name, address, deviceRepository, bleService)
    {
        _availableMacros = StaticMacros;
    }

    public sealed override bool SupportsMacros => true;
    public sealed override IReadOnlyList<MacroDescriptor> AvailableMacros => _availableMacros;

    protected virtual IReadOnlyList<MacroDescriptor> StaticMacros => [];

    public sealed override async ValueTask<IReadOnlyList<MacroDescriptor>> GetMacrosAsync(bool forceRefresh = false, CancellationToken token = default)
    {
        if (!forceRefresh || !SupportsDynamicMacros || DeviceState != DeviceState.Connected)
        {
            return AvailableMacros;
        }

        using (await _asyncLock.LockAsync(token))
        {
            if (DeviceState != DeviceState.Connected)
            {
                return AvailableMacros;
            }

            var discoveredMacros = await DiscoverDynamicMacrosAsync(token);
            UpdateMacroCache(discoveredMacros);
        }

        return AvailableMacros;
    }

    protected virtual ValueTask<IReadOnlyList<MacroDescriptor>> DiscoverDynamicMacrosAsync(CancellationToken token)
        => ValueTask.FromResult<IReadOnlyList<MacroDescriptor>>([]);

    protected void UpdateMacroCache(IReadOnlyList<MacroDescriptor>? dynamicMacros)
    {
        dynamicMacros ??= [];

        _availableMacros = [.. StaticMacros, .. dynamicMacros];
        RaisePropertyChanged(nameof(AvailableMacros));
    }
}
