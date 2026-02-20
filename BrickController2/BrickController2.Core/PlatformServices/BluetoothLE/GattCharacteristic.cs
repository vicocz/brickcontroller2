using BrickController2.Common;
using BrickController2.PlatformServices.BluetoothLE;
using Plugin.BLE.Abstractions.EventArgs;
using BLE = Plugin.BLE.Abstractions.Contracts;
namespace BrickController2.Core.PlatformServices.BluetoothLE;

internal class GattCharacteristic : AsyncDisposableBase, IGattCharacteristic
{
    private BLE.ICharacteristic? _characteristic;
    private bool _notificationEnabled;

    public GattCharacteristic(BLE.ICharacteristic characteristic)
    {
        _characteristic = characteristic;
    }

    public Guid Uuid => _characteristic!.Id;

    public bool CanNotify => _characteristic?.CanUpdate ?? false;

    public async Task<bool> WriteValueAsync(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(_characteristic);

        return await _characteristic.WriteAsync(data) == 0;
    }

    public async Task<bool> ReadValueAsync(CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(_characteristic);

        var (data, resultCode) = await _characteristic.ReadAsync(token);
        return resultCode == 0;
    }

    public async Task<bool> EnableNotificationAsync(Action<Guid, byte[]> callback, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ArgumentNullException.ThrowIfNull(_characteristic);

        try
        {
            _characteristic.ValueUpdated += ValueUpdatedHandler;

            await _characteristic.StartUpdatesAsync(token);
            _notificationEnabled = true;

            return true;
        }
        catch
        {
            _characteristic.ValueUpdated -= ValueUpdatedHandler;
            return false;
        }

        void ValueUpdatedHandler(object? sender, CharacteristicUpdatedEventArgs e)
        {
            callback(e.Characteristic.Id, e.Characteristic.Value);
        }
    }

    protected override void Dispose(bool disposing)
    {
        _characteristic = null;
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        // silently try to stop notifications if they were enabled,
        // but ignore any errors since we're disposing anyway
        if (_characteristic != null && _notificationEnabled)
        {
            try
            {
                await _characteristic.StopUpdatesAsync();
            }
            catch
            {
            }
        }
    }
}