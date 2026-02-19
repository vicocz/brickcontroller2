using System;
namespace BrickController2.PlatformServices.BluetoothLE
{
    public interface IGattService
    {
        Guid Uuid { get; }

        bool ContainsCharacteristic(Guid characteristicUuid);
    }
}
