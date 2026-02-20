using BrickController2.Common;
using BrickController2.PlatformServices.BluetoothLE;

using BLE = Plugin.BLE.Abstractions.Contracts;

namespace BrickController2.Core.PlatformServices.BluetoothLE;
internal class BleGattService : AsyncDisposableBase, IGattService
{
    private BLE.IService? _service;
    private IReadOnlyCollection<GattCharacteristic>? _characteristics;

    public BleGattService(BLE.IService service, IReadOnlyCollection<GattCharacteristic> characteristics)
    {
        _service = service;
        _characteristics = characteristics;
    }

    public Guid Uuid => _service!.Id;

    public IEnumerable<IGattCharacteristic> Characteristics => _characteristics ?? [];

    protected override void Dispose(bool disposing)
    {
        _service?.Dispose();
        _service = null;
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_characteristics != null)
        {
            foreach (var characteristic in _characteristics)
            {
                await characteristic.DisposeAsync();
            }
            _characteristics = null;
        }
    }
}