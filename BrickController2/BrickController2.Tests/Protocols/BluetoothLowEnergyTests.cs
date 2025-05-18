using BrickController2.Protocols;
using FluentAssertions;
using System;
using Xunit;

namespace BrickController2.Tests.Protocols;

public class BluetoothLowEnergyTests
{
    [Fact]
    public void GetGuid_Ble128BitByteArray_GuidProperlyReturned()
    {
        var guid = BluetoothLowEnergy.GetGuid([0x23, 0xd1, 0xbc, 0xea, 0x5f, 0x78, 0x23, 0x15, 0xde, 0xef, 0x12, 0x12, 0x23, 0x15, 0x00, 0x00]);
        guid.Should().Be(new Guid("00001523-1212-efde-1523-785feabcd123"));
    }

    [Fact]
    public void To128BitByteArray_ServiceUuid_Ble128BitByteArrayReturned()
    {
        var byteArray = BluetoothLowEnergy.To128BitByteArray(new Guid("00001523-1212-efde-1523-785feabcd123"));
        byteArray.Should().NotBeNull().And.HaveCount(16);
        byteArray.Should().BeEquivalentTo([0x23, 0xd1, 0xbc, 0xea, 0x5f, 0x78, 0x23, 0x15, 0xde, 0xef, 0x12, 0x12, 0x23, 0x15, 0x00, 0x00]);
    }
}
