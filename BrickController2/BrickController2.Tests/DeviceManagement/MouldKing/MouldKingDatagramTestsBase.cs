using BrickController2.DeviceManagement.MouldKing;
using BrickController2.PlatformServices.BluetoothLE;
using Moq;

namespace BrickController2.Tests.DeviceManagement.MouldKing;

public abstract class MouldKingDatagramTestsBase
{
    protected const byte AppIdentifier1 = 0x61; // This is the first byte of an randomly choosen AppIdentifier for UnitTesting
    protected const byte AppIdentifier2 = 0x62; // This is the second byte of an randomly choosen AppIdentifier for UnitTesting

    protected static readonly byte[] AppIdentifier = [AppIdentifier1, AppIdentifier2];

    /// <summary>
    /// This class is a test implementation of the IMKPlatformService interface that simulates the behavior of the TryGetRfPayload method for testing purposes.
    /// It always returns true and sets the rfPayload to the rawData provided.
    /// </summary>
    protected class TestMKPlatformService : IMKPlatformService
    {
        public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload)
        {
            rfPayload = rawData;
            return true; // Simulate success
        }
    }

    protected readonly Mock<IBluetoothLEService> _bluetoothLEService = new(MockBehavior.Strict);
    protected readonly Mock<IMouldKingDeviceManager> _manager = new(MockBehavior.Strict);
    protected readonly TestMKPlatformService _mkPlatformService = new();

    protected MouldKingDatagramTestsBase()
    {
        _manager.Setup(x => x.GetAppId()).Returns(AppIdentifier);
    }
}
