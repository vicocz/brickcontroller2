using BrickController2.PlatformServices.BluetoothLE;
using Plugin.BLE.Abstractions.EventArgs;
using BLE = Plugin.BLE.Abstractions.Contracts;
namespace BrickController2.Core.PlatformServices.BluetoothLE;

internal class GattCharacteristic : IGattCharacteristic
{
    private readonly BLE.ICharacteristic _characteristic;

    public GattCharacteristic(BLE.ICharacteristic characteristic)
    {
        _characteristic = characteristic;
    }

    public Guid Uuid => _characteristic.Id;

    public bool CanNotify => _characteristic.CanUpdate;
    public async Task<bool> WriteValueAsync(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return await _characteristic.WriteAsync(data) == 0;
    }

    public async Task<bool> ReadValueAsync(CancellationToken token = default)
    {
        var (data, resultCode) = await _characteristic.ReadAsync(token);
        return resultCode == 0;
    }

    public async Task<bool> EnableNotificationAsync(Action<Guid, byte[]> callback,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _characteristic.ValueUpdated += ValueUpdatedHandler;

        void ValueUpdatedHandler(object? sender, CharacteristicUpdatedEventArgs e)
        {
            callback(e.Characteristic.Id, e.Characteristic.Value);
        }

        await _characteristic.StartUpdatesAsync(token);

        return true;
    }
}