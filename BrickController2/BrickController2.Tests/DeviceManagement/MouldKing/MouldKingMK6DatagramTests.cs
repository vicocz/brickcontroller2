using BrickController2.DeviceManagement.MouldKing;
using FluentAssertions;
using System;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.MouldKing;

public sealed class MouldKingMK6DatagramTests : MouldKingDatagramTestsBase
{
    private const byte PayloadIdentifierConnect1 = 0x6d;
    private const byte PayloadIdentifierConnect2 = 0x92;
    private const byte PayloadIdentifierCommand1_1 = 0x61;
    private const byte PayloadIdentifierCommand1_2 = 0x9e;
    private const byte PayloadIdentifierCommand2_1 = 0x62;
    private const byte PayloadIdentifierCommand2_2 = 0x9d;
    private const byte PayloadIdentifierCommand3_1 = 0x63;
    private const byte PayloadIdentifierCommand3_2 = 0x9c;

    /// <summary>
    /// This test checks that the payload identifiers are correctly set in the connect datagram for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(MK6.Device1)]
    [InlineData(MK6.Device2)]
    [InlineData(MK6.Device3)]
    public void TryGetTelegram_ConnectDatagram_PayloadIdentifier(string deviceAddress)
    {
        MK6 device = new MK6("MK6", deviceAddress, [], _deviceRepository.Object, _bluetoothLEService.Object, _mkPlatformService, _manager.Object);

        device.TryGetTelegram(true, out byte[] payload).Should().BeTrue();
        payload[0].Should().Be(PayloadIdentifierConnect1);
        payload[7].Should().Be(PayloadIdentifierConnect2);
    }

    /// <summary>
    /// This test checks that the AppIdentifier is correctly included in the connect datagram payload for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(MK6.Device1)]
    [InlineData(MK6.Device2)]
    [InlineData(MK6.Device3)]
    public void TryGetTelegram_ConnectDatagram_AppIdentifier(string deviceAddress)
    {
        MK6 device = new MK6("MK6", deviceAddress, [], _deviceRepository.Object, _bluetoothLEService.Object, _mkPlatformService, _manager.Object);

        device.TryGetTelegram(true, out byte[] payload).Should().BeTrue();
        payload[1].Should().Be(AppIdentifier1);
        payload[2].Should().Be(AppIdentifier2);
    }

    /// <summary>
    /// This test checks that the payload identifiers are correctly set in the command datagram for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    /// <param name="expectedPayloadIdentifier1">The expected first payload identifier.</param>
    /// <param name="expectedPayloadIdentifier2">The expected second payload identifier.</param>
    [Theory]
    [InlineData(MK6.Device1, PayloadIdentifierCommand1_1, PayloadIdentifierCommand1_2)]
    [InlineData(MK6.Device2, PayloadIdentifierCommand2_1, PayloadIdentifierCommand2_2)]
    [InlineData(MK6.Device3, PayloadIdentifierCommand3_1, PayloadIdentifierCommand3_2)]
    public void TryGetTelegram_CommandDatagram_PayloadIdentifier(string deviceAddress, byte expectedPayloadIdentifier1, byte expectedPayloadIdentifier2)
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
    public void TryGetTelegram_CommandDatagram_AppIdentifier(string deviceAddress)
    {
        MK6 device = new MK6("MK6", deviceAddress, [], _deviceRepository.Object, _bluetoothLEService.Object, _mkPlatformService, _manager.Object);

        device.TryGetTelegram(false, out byte[] payload).Should().BeTrue();
        payload[1].Should().Be(AppIdentifier1);
        payload[2].Should().Be(AppIdentifier2);
    }

    /// <summary>
    /// This test checks that the command datagram payload is correctly constructed based on the set output values for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    /// <param name="setValues">The set values for each channel.</param>
    /// <param name="expectedPayload">The expected payload for the command datagram.</param>
    [Theory]
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1_1, AppIdentifier1, AppIdentifier2, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand1_2 })]        // all channels neutral
    [InlineData(MK6.Device1, new float[] { 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1_1, AppIdentifier1, AppIdentifier2, 0xff, 0x80, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand1_2 })]        // channel 1 maximum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1_1, AppIdentifier1, AppIdentifier2, 0x80, 0xff, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand1_2 })]        // channel 2 maximum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1_1, AppIdentifier1, AppIdentifier2, 0x80, 0x80, 0xff, 0x80, 0x80, 0x80, PayloadIdentifierCommand1_2 })]        // channel 3 maximum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1_1, AppIdentifier1, AppIdentifier2, 0x80, 0x80, 0x80, 0xff, 0x80, 0x80, PayloadIdentifierCommand1_2 })]        // channel 4 maximum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1_1, AppIdentifier1, AppIdentifier2, 0x80, 0x80, 0x80, 0x80, 0xff, 0x80, PayloadIdentifierCommand1_2 })]        // channel 5 maximum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f }, new byte[] { PayloadIdentifierCommand1_1, AppIdentifier1, AppIdentifier2, 0x80, 0x80, 0x80, 0x80, 0x80, 0xff, PayloadIdentifierCommand1_2 })]        // channel 6 maximum, others neutral
    [InlineData(MK6.Device1, new float[] { -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1_1, AppIdentifier1, AppIdentifier2, 0x00, 0x80, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand1_2 })]       // channel 1 minimum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1_1, AppIdentifier1, AppIdentifier2, 0x80, 0x00, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand1_2 })]       // channel 2 minimum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1_1, AppIdentifier1, AppIdentifier2, 0x80, 0x80, 0x00, 0x80, 0x80, 0x80, PayloadIdentifierCommand1_2 })]       // channel 3 minimum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1_1, AppIdentifier1, AppIdentifier2, 0x80, 0x80, 0x80, 0x00, 0x80, 0x80, PayloadIdentifierCommand1_2 })]       // channel 4 minimum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1_1, AppIdentifier1, AppIdentifier2, 0x80, 0x80, 0x80, 0x80, 0x00, 0x80, PayloadIdentifierCommand1_2 })]       // channel 5 minimum, others neutral
    [InlineData(MK6.Device1, new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f }, new byte[] { PayloadIdentifierCommand1_1, AppIdentifier1, AppIdentifier2, 0x80, 0x80, 0x80, 0x80, 0x80, 0x00, PayloadIdentifierCommand1_2 })]       // channel 6 minimum, others neutral
    [InlineData(MK6.Device1, new float[] { 9.0f, 9.0f, 9.0f, 9.0f, 9.0f, 9.0f }, new byte[] { PayloadIdentifierCommand1_1, AppIdentifier1, AppIdentifier2, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, PayloadIdentifierCommand1_2 })]        // all channels above maximum, should be clamped to maximum
    [InlineData(MK6.Device1, new float[] { -9.0f, -9.0f, -9.0f, -9.0f, -9.0f, -9.0f }, new byte[] { PayloadIdentifierCommand1_1, AppIdentifier1, AppIdentifier2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, PayloadIdentifierCommand1_2 })]  // all channels below minimum, should be clamped to minimum

    [InlineData(MK6.Device2, new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand2_1, AppIdentifier1, AppIdentifier2, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand2_2 })]        // all channels neutral
    [InlineData(MK6.Device2, new float[] { 9.0f, 9.0f, 9.0f, 9.0f, 9.0f, 9.0f }, new byte[] { PayloadIdentifierCommand2_1, AppIdentifier1, AppIdentifier2, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, PayloadIdentifierCommand2_2 })]        // all channels above maximum, should be clamped to maximum
    [InlineData(MK6.Device2, new float[] { -9.0f, -9.0f, -9.0f, -9.0f, -9.0f, -9.0f }, new byte[] { PayloadIdentifierCommand2_1, AppIdentifier1, AppIdentifier2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, PayloadIdentifierCommand2_2 })]  // all channels below minimum, should be clamped to minimum

    [InlineData(MK6.Device3, new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand3_1, AppIdentifier1, AppIdentifier2, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, PayloadIdentifierCommand3_2 })]        // all channels neutral
    [InlineData(MK6.Device3, new float[] { 9.0f, 9.0f, 9.0f, 9.0f, 9.0f, 9.0f }, new byte[] { PayloadIdentifierCommand3_1, AppIdentifier1, AppIdentifier2, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, PayloadIdentifierCommand3_2 })]        // all channels above maximum, should be clamped to maximum
    [InlineData(MK6.Device3, new float[] { -9.0f, -9.0f, -9.0f, -9.0f, -9.0f, -9.0f }, new byte[] { PayloadIdentifierCommand3_1, AppIdentifier1, AppIdentifier2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, PayloadIdentifierCommand3_2 })]  // all channels below minimum, should be clamped to minimum
    public void TryGetTelegram_CommandDatagram_Payload(string deviceAddress, float[] setValues, byte[] expectedPayload)
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
            payload[i].Should().Be(expectedPayload[i]);
        }
    }

    /// <summary>
    /// Tests that setting an illegal channel index throws an ArgumentOutOfRangeException.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(MK6.Device1)]
    [InlineData(MK6.Device2)]
    [InlineData(MK6.Device3)]
    public void TryGetTelegram_CommandDatagram_SetIllegalChannel(string deviceAddress)
    {
        MK6 device = new MK6("MK6", deviceAddress, [], _deviceRepository.Object, _bluetoothLEService.Object, _mkPlatformService, _manager.Object);

        Action action = () => device.SetOutput(device.NumberOfChannels, 0);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
