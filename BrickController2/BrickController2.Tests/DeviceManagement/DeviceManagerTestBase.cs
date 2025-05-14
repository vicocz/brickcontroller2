using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using System;
using System.Collections.Generic;

namespace BrickController2.Tests.DeviceManagement;

public abstract class DeviceManagerTestBase<TManager> where TManager : IBluetoothLEDeviceManager, new()
{
    protected readonly TManager _manager = new();

    protected static ScanResult CreateScanResult(string? deviceName, Guid serviceUuid)
        => CreateScanResult(deviceName, new Dictionary<byte, byte[]>()
        {
            { 0x06, serviceUuid.To128BitByteArray() }
        });

    protected static ScanResult CreateScanResult(string? deviceName, byte[] manufacturerData)
        => CreateScanResult(deviceName, new Dictionary<byte, byte[]>()
        {
            { 0xFF, manufacturerData }
        });

    protected static ScanResult CreateScanResult(string? deviceName)
       => CreateScanResult(deviceName, advertismentData:default);

    protected static ScanResult CreateScanResult(string? deviceName, Dictionary<byte, byte[]>? advertismentData = default)
        => new(deviceName ?? typeof(TManager).Name, Random.Shared.Next().ToString("X"), advertismentData ?? []);
}
