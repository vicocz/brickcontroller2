using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.CaDA;
using BrickController2.PlatformServices.BluetoothLE;
using Moq;
using System;

namespace BrickController2.Tests.DeviceManagement.CaDA;

public abstract class CaDADatagramTestsBase
{
    protected const byte AppIdentifier1 = 0x61; // This is the first byte of an randomly choosen AppIdentifier for UnitTesting
    protected const byte AppIdentifier2 = 0x62; // This is the second byte of an randomly choosen AppIdentifier for UnitTesting
    protected const byte AppIdentifier3 = 0x63; // This is the third byte of an randomly choosen AppIdentifier for UnitTesting

    protected static readonly byte[] AppIdentifier = [AppIdentifier1, AppIdentifier2, AppIdentifier3];

    /// <summary>
    /// This class is a test implementation of the ICaDAPlatformService interface that simulates the behavior of the TryGetRfPayload method for testing purposes.
    /// It always returns true and sets the rfPayload to the rawData provided.
    /// </summary>
    protected class TestCaDAPlatformService : ICaDAPlatformService
    {
        public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload)
        {
            rfPayload = rawData;
            return true; // Simulate success
        }
    }

    protected readonly Mock<IBluetoothLEService> _bluetoothLEService = new(MockBehavior.Strict);
    protected readonly Mock<ICaDADeviceManager> _manager = new(MockBehavior.Strict);
    protected readonly TestCaDAPlatformService _cadaPlatformService = new();
    internal readonly Mock<IDeviceRepository> _deviceRepository = new(MockBehavior.Strict);
    protected readonly MessageEncoderFactory _messageEncoderFactory;
    protected readonly Mock<Random> _random = new(MockBehavior.Strict);

    protected CaDADatagramTestsBase()
    {
        _manager.Setup(x => x.GetAppId()).Returns(AppIdentifier);
        _random.Setup(x => x.Next(ushort.MinValue, ushort.MaxValue)).Returns(0); // mock random number generation to always return 0 for testing
        _messageEncoderFactory = new MessageEncoderFactory(_manager.Object, _cadaPlatformService, _random.Object);
    }
}
