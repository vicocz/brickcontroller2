using System;
using System.Collections.Generic;
using System.Linq;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Extensions;

internal static class GattServiceExtensions
{
    /// <summary>
    /// Gets a characteristic from the services collection based on service and characteristic UUID.
    /// </summary>
    /// <param name="services">The collection of GATT services.</param>
    /// <param name="serviceUuid">The UUID of the service.</param>
    /// <param name="characteristicUuid">The UUID of the characteristic.</param>
    /// <returns>The matching characteristic, or null if not found.</returns>
    public static IGattCharacteristic? GetCharacteristic(
        this IEnumerable<IGattService>? services,
        Guid serviceUuid,
        Guid characteristicUuid)
    {
        var service = services?.FirstOrDefault(s => s.Uuid == serviceUuid);
        return service?.Characteristics?.FirstOrDefault(c => c.Uuid == characteristicUuid);
    }
}