using System;
using System.Collections.Generic;
using System.Linq;
using BrickController2.PlatformServices.BluetoothLE;
using CoreBluetooth;

namespace BrickController2.iOS.PlatformServices.BluetoothLE
{
    internal class GattService : IGattService
    {
        private readonly IReadOnlySet<Guid> _characteristics;

        public GattService(CBService service, IEnumerable<CBCharacteristic> characteristics)
        {
            Service = service;
            _characteristics = characteristics
                .Select(ch => ch.UUID.ToGuid())
                .ToHashSet();
        }

        public CBService Service { get; }
        public Guid Uuid => Service.UUID.ToGuid();

        public bool ContainsCharacteristic(Guid characteristicUuid) => _characteristics.Contains(characteristicUuid);
    }
}