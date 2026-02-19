using BrickController2.PlatformServices.BluetoothLE;

using BLE = Plugin.BLE.Abstractions.Contracts;

namespace BrickController2.Core.PlatformServices.BluetoothLE;
internal class BleGattService : IGattService, IDisposable
{
    private BLE.IService? _service;

    public BleGattService(BLE.IService service, IReadOnlyCollection<IGattCharacteristic> characteristics)
    {
        _service = service;
        Characteristics = characteristics;
    }

    public Guid Uuid => _service!.Id;

    public IEnumerable<IGattCharacteristic> Characteristics { get; }

    #region IDisposable

    ~BleGattService() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _service?.Dispose();
        _service = null;
    }

    #endregion
}