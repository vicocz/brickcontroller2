using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.MouldKing;
using BrickController2.PlatformServices.BluetoothLE;
using FluentAssertions;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.MouldKing;

public class MouldKing_MK6_DatagramTests
{
    private static readonly byte[] AppIdentifier = [0x61, 0x62];

    /// <summary>
    /// This class is a test implementation of the IMKPlatformService interface that simulates the behavior of the TryGetRfPayload method for testing purposes.
    /// It always returns true and sets the rfPayload to the rawData provided.
    /// </summary>
    private class TestMKPlatformService : IMKPlatformService
    {
        public bool TryGetRfPayload(byte[] rawData, out byte[] rfPayload)
        {
            rfPayload = rawData;
            return true; // Simulate success
        }
    }

    private readonly Mock<IDeviceRepository> _deviceRepository = new(MockBehavior.Strict);
    private readonly Mock<IBluetoothLEService> _bluetoothLEService = new(MockBehavior.Strict);
    private readonly Mock<IMouldKingDeviceManager> _manager = new(MockBehavior.Strict);
    private readonly TestMKPlatformService _mkPlatformService = new();

    public MouldKing_MK6_DatagramTests()
    {
        _manager.Setup(x => x.GetAppId()).Returns(AppIdentifier);
    }

    /// <summary>
    /// This test checks that the payload identifiers are correctly set in the connect datagram for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    /// <param name="expectedPayloadIdentifier1">The expected first payload identifier.</param>
    /// <param name="expectedPayloadIdentifier2">The expected second payload identifier.</param>
    [Theory]
    [InlineData(MK6.Device1, 0x6d, 0x92)]
    [InlineData(MK6.Device2, 0x6d, 0x92)]
    [InlineData(MK6.Device3, 0x6d, 0x92)]
    public void MK6_TryGetTelegram_ConnectDatagram_PayloadIdentifier(string deviceAddress, byte expectedPayloadIdentifier1, byte expectedPayloadIdentifier2)
    {
        MK6 device = new MK6("MK6", deviceAddress, [], _deviceRepository.Object, _bluetoothLEService.Object, _mkPlatformService, _manager.Object);

        device.TryGetTelegram(true, out byte[] payload).Should().BeTrue();
        payload[0].Should().Be(expectedPayloadIdentifier1);
        payload[7].Should().Be(expectedPayloadIdentifier2);
    }

    /// <summary>
    /// This test checks that the AppIdentifier is correctly included in the connect datagram payload for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(MK6.Device1)]
    [InlineData(MK6.Device2)]
    [InlineData(MK6.Device3)]
    public void MK6_TryGetTelegram_ConnectDatagram_AppIdentifier(string deviceAddress)
    {
        MK6 device = new MK6("MK6", deviceAddress, [], _deviceRepository.Object, _bluetoothLEService.Object, _mkPlatformService, _manager.Object);

        device.TryGetTelegram(true, out byte[] payload).Should().BeTrue();
        payload[1].Should().Be(AppIdentifier[0]);
        payload[2].Should().Be(AppIdentifier[1]);
    }

    /// <summary>
    /// This test checks that the payload identifiers are correctly set in the command datagram for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    /// <param name="expectedPayloadIdentifier1">The expected first payload identifier.</param>
    /// <param name="expectedPayloadIdentifier2">The expected second payload identifier.</param>
    [Theory]
    [InlineData(MK6.Device1, 0x61, 0x9e)]
    [InlineData(MK6.Device2, 0x62, 0x9d)]
    [InlineData(MK6.Device3, 0x63, 0x9c)]
    public async Task MK6_TryGetTelegram_CommandDatagram_PayloadIdentifier(string deviceAddress, byte expectedPayloadIdentifier1, byte expectedPayloadIdentifier2)
    {
        MK6 device = new MK6("MK6", deviceAddress, [], _deviceRepository.Object, _bluetoothLEService.Object, _mkPlatformService, _manager.Object);

        device.TryGetTelegram(false, out byte[] payload).Should().BeTrue();
        payload[0].Should().Be(expectedPayloadIdentifier1);
        payload[9].Should().Be(expectedPayloadIdentifier2);
    }

    /// <summary>
    /// This test checks that the AppIdentifier is correctly included in the command datagram payload for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(MK6.Device1)]
    [InlineData(MK6.Device2)]
    [InlineData(MK6.Device3)]
    public async Task MK6_TryGetTelegram_CommandDatagram_AppIdentifier(string deviceAddress)
    {
        MK6 device = new MK6("MK6", deviceAddress, [], _deviceRepository.Object, _bluetoothLEService.Object, _mkPlatformService, _manager.Object);

        device.TryGetTelegram(false, out byte[] payload).Should().BeTrue();
        payload[1].Should().Be(AppIdentifier[0]);
        payload[2].Should().Be(AppIdentifier[1]);
    }

    /// <summary>
    /// This test checks that the command datagram payload is correctly constructed based on the set output values for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    /// <param name="setValues">The set values for each channel.</param>
    /// <param name="expectedPayload">The expected payload for the command datagram.</param>
    [Theory]
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 })]        // all channels neutral
    [InlineData(MK6.Device1, new float[] { 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { 0xff, 0x80, 0x80, 0x80, 0x80, 0x80 })]        // channel 1 maximum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { 0x80, 0xff, 0x80, 0x80, 0x80, 0x80 })]        // channel 2 maximum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f }, new byte[] { 0x80, 0x80, 0xff, 0x80, 0x80, 0x80 })]        // channel 3 maximum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f }, new byte[] { 0x80, 0x80, 0x80, 0xff, 0x80, 0x80 })]        // channel 4 maximum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f }, new byte[] { 0x80, 0x80, 0x80, 0x80, 0xff, 0x80 })]        // channel 5 maximum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f }, new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0xff })]        // channel 6 maximum, others neutral
    [InlineData(MK6.Device1, new float[] { -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { 0x00, 0x80, 0x80, 0x80, 0x80, 0x80 })]       // channel 1 minimum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { 0x80, 0x00, 0x80, 0x80, 0x80, 0x80 })]       // channel 2 minimum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f }, new byte[] { 0x80, 0x80, 0x00, 0x80, 0x80, 0x80 })]       // channel 3 minimum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f }, new byte[] { 0x80, 0x80, 0x80, 0x00, 0x80, 0x80 })]       // channel 4 minimum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f }, new byte[] { 0x80, 0x80, 0x80, 0x80, 0x00, 0x80 })]       // channel 5 minimum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f }, new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x00 })]       // channel 6 minimum, others neutral
    [InlineData(MK6.Device1, new float[] { 9.0f, 9.0f, 9.0f, 9.0f, 9.0f, 9.0f }, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff })]        // all channels above maximum, should be clamped to maximum
    [InlineData(MK6.Device1, new float[] { -9.0f, -9.0f, -9.0f, -9.0f, -9.0f, -9.0f }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })]  // all channels below minimum, should be clamped to minimum

    [InlineData(MK6.Device2, new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 })]        // all channels neutral
    [InlineData(MK6.Device2, new float[] { 9.0f, 9.0f, 9.0f, 9.0f, 9.0f, 9.0f }, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff })]        // all channels above maximum, should be clamped to maximum
    [InlineData(MK6.Device2, new float[] { -9.0f, -9.0f, -9.0f, -9.0f, -9.0f, -9.0f }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })]  // all channels below minimum, should be clamped to minimum

    [InlineData(MK6.Device3, new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 })]        // all channels neutral
    [InlineData(MK6.Device3, new float[] { 9.0f, 9.0f, 9.0f, 9.0f, 9.0f, 9.0f }, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff })]        // all channels above maximum, should be clamped to maximum
    [InlineData(MK6.Device3, new float[] { -9.0f, -9.0f, -9.0f, -9.0f, -9.0f, -9.0f }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })]  // all channels below minimum, should be clamped to minimum
    public async Task MK6_Check_CommandDatagram_Payload(string deviceAddress, float[] setValues, byte[] expectedPayload)
    {
        MK6 device = new MK6("MK6", deviceAddress, [], _deviceRepository.Object, _bluetoothLEService.Object, _mkPlatformService, _manager.Object);

        // Set the output values for the device
        for (int i = 0; i < setValues.Length; i++)
        {
            device.SetOutput(i, setValues[i]);
        }

        // Get the command datagram payload
        device.TryGetTelegram(false, out byte[] payload).Should().BeTrue();

        // Check that the payload matches the expected values
        for (int i = 0; i < expectedPayload.Length; i++)
        {
            payload[i + 3].Should().Be(expectedPayload[i]);
        }
    }
}
