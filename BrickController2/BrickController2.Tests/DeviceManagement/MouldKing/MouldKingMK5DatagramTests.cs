using BrickController2.DeviceManagement.MouldKing;
using FluentAssertions;
using System;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.MouldKing;

public sealed class MouldKingMK5DatagramTests : MouldKingDatagramTestsBase
{
    private const byte PayloadIdentifierConnect1 = 0xad;
    private const byte PayloadIdentifierConnect2 = 0x52;
    private const byte PayloadIdentifierCommand1 = 0x7d;
    private const byte PayloadIdentifierCommand2 = 0x82;

    /// <summary>
    /// This test checks that the payload identifiers are correctly set in the connect datagram for each device address.
    /// </summary>
    [Fact]
    public void TryGetTelegram_ConnectDatagram_PayloadIdentifier()
    {
        MK5 device = new MK5("MK5", MK5.Device, [], _deviceRepository.Object, _bluetoothLEService.Object, _mkPlatformService, _manager.Object);

        device.TryGetTelegram(true, out byte[] payload).Should().BeTrue();
        payload[0].Should().Be(PayloadIdentifierConnect1);
        payload[7].Should().Be(PayloadIdentifierConnect2);
    }

    /// <summary>
    /// This test checks that the AppIdentifier is correctly included in the connect datagram payload for each device address.
    /// </summary>
    [Fact]
    public void TryGetTelegram_ConnectDatagram_AppIdentifier()
    {
        MK5 device = new MK5("MK5", MK5.Device, [], _deviceRepository.Object, _bluetoothLEService.Object, _mkPlatformService, _manager.Object);

        device.TryGetTelegram(true, out byte[] payload).Should().BeTrue();
        payload[1].Should().Be(AppIdentifier1);
        payload[2].Should().Be(AppIdentifier2);
    }

    /// <summary>
    /// This test checks that the payload identifiers are correctly set in the command datagram for each device address.
    /// </summary>
    [Fact]
    public void TryGetTelegram_CommandDatagram_PayloadIdentifier()
    {
        MK5 device = new MK5("MK5", MK5.Device, [], _deviceRepository.Object, _bluetoothLEService.Object, _mkPlatformService, _manager.Object);

        device.TryGetTelegram(false, out byte[] payload).Should().BeTrue();
        payload[0].Should().Be(PayloadIdentifierCommand1);
        payload[9].Should().Be(PayloadIdentifierCommand2);
    }

    /// <summary>
    /// This test checks that the AppIdentifier is correctly included in the command datagram payload for each device address.
    /// </summary>
    [Fact]
    public void TryGetTelegram_CommandDatagram_AppIdentifier()
    {
        MK5 device = new MK5("MK5", MK5.Device, [], _deviceRepository.Object, _bluetoothLEService.Object, _mkPlatformService, _manager.Object);

        device.TryGetTelegram(false, out byte[] payload).Should().BeTrue();
        payload[1].Should().Be(AppIdentifier1);
        payload[2].Should().Be(AppIdentifier2);
    }

    /// <summary>
    /// This test checks that the command datagram payload is correctly constructed based on the set output values for each device address.
    /// </summary>
    /// <param name="setValues">The set values for each channel.</param>
    /// <param name="expectedPayload">The expected payload for the command datagram.</param>
    [Theory]
    [InlineData(new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x00, 0x00, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand2 })]        // all channels neutral
    [InlineData(new float[] { 1.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0xf0, 0x00, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand2 })]        // channel 1 maximum, others neutral
    [InlineData(new float[] { 0.0f, 1.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x07, 0x00, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand2 })]        // channel 2 maximum, others neutral
    [InlineData(new float[] { 0.0f, 0.0f, 1.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x00, 0xf0, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand2 })]        // channel 3 maximum, others neutral
    [InlineData(new float[] { 0.0f, 0.0f, 0.0f, 1.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x00, 0x0f, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand2 })]        // channel 4 maximum, others neutral
    [InlineData(new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x02, 0x00, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand2 })]        // channel 5 maximum, others neutral
    [InlineData(new float[] { -1.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x70, 0x00, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand2 })]       // channel 1 minimum, others neutral
    [InlineData(new float[] { 0.0f, -1.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x0f, 0x00, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand2 })]       // channel 2 minimum, others neutral
    [InlineData(new float[] { 0.0f, 0.0f, -1.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x00, 0xf0, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand2 })]       // channel 3 minimum, others neutral
    [InlineData(new float[] { 0.0f, 0.0f, 0.0f, -1.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x00, 0x07, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand2 })]       // channel 4 minimum, others neutral
    [InlineData(new float[] { 0.0f, 0.0f, 0.0f, 0.0f, -1.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x02, 0x00, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand2 })]       // channel 5 minimum, others neutral
    [InlineData(new float[] { 9.0f, 9.0f, 9.0f, 9.0f, 9.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0xf7, 0xff, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand2 })]        // all channels above maximum, should be clamped to maximum
    [InlineData(new float[] { -9.0f, -9.0f, -9.0f, -9.0f, -9.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x7f, 0xf7, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand2 })]  // all channels below minimum, should be clamped to minimum
    public void TryGetTelegram_CommandDatagram_Payload(float[] setValues, byte[] expectedPayload)
    {
        MK5 device = new MK5("MK5", MK5.Device, [], _deviceRepository.Object, _bluetoothLEService.Object, _mkPlatformService, _manager.Object);

        // Set the output values for the device
        for (int i = 0; i < setValues.Length; i++)
        {
            device.SetOutput(i, setValues[i]);
        }

        // Get the command datagram payload
        device.TryGetTelegram(false, out byte[] payload).Should().BeTrue();

        // Check that the payload matches the expected values
        payload.Should().BeEquivalentTo(expectedPayload);
    }

    /// <summary>
    /// Tests that setting an illegal channel index throws an ArgumentOutOfRangeException.
    /// </summary>
    [Fact]
    public void TryGetTelegram_CommandDatagram_SetIllegalChannel()
    {
        MK5 device = new MK5("MK5", MK5.Device, [], _deviceRepository.Object, _bluetoothLEService.Object, _mkPlatformService, _manager.Object);

        Action action = () => device.SetOutput(device.NumberOfChannels, 0);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
