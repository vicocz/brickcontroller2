using BrickController2.DeviceManagement.CaDA;
using FluentAssertions;
using System;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.CaDA;

public sealed class CaDARaceCarRev2DatagramTests : CaDADatagramTestsBase
{
    private const byte PayloadPairingIdentifier1 = 0xaa;
    private const byte PayloadCommandIdentifier1 = 0xbb;
    private const byte PayloadIdentifier2 = 0x11;
    private static readonly byte[] PayloadPairingFooter = [0xcc, 0xb8, 0x92, 0xa0];
    private static readonly byte[] PayloadCommandFooter = [0xcc, 0xb8, 0x92, 0xb0];

    /// <summary>
    /// This test checks that the payload identifiers are correctly set in the connect datagram for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(new byte[] { 0xab, 0xcd })]
    [InlineData(new byte[] { 0x12, 0x34 })]
    public void TryGetTelegram_ConnectDatagram_PayloadIdentifier(byte[] deviceAddress)
    {
        byte[] scanData = [0x00, 0x00, 0x00, 0x00, 0x00, deviceAddress[0], deviceAddress[1], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        CaDARaceCar device = new CaDARaceCar("CaDARaceCar", BitConverter.ToString(deviceAddress).ToLower(), scanData, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);

        device.TryGetTelegram(true, out byte[] payload).Should().BeTrue();

        payload[0].Should().Be(PayloadPairingIdentifier1);
        payload[1].Should().Be(PayloadIdentifier2);
        payload[12].Should().Be(PayloadPairingFooter[0]);
        payload[13].Should().Be(PayloadPairingFooter[1]);
        payload[14].Should().Be(PayloadPairingFooter[2]);
        payload[15].Should().Be(PayloadPairingFooter[3]);
    }

    /// <summary>
    /// This test checks that the AppIdentifier is correctly included in the connect datagram payload for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(new byte[] { 0xab, 0xcd })]
    [InlineData(new byte[] { 0x12, 0x34 })]
    public void TryGetTelegram_ConnectDatagram_AppIdentifier(byte[] deviceAddress)
    {
        byte[] scanData = [0x00, 0x00, 0x00, 0x00, 0x00, deviceAddress[0], deviceAddress[1], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
        CaDARaceCar device = new CaDARaceCar("CaDARaceCar", BitConverter.ToString(deviceAddress).ToLower(), scanData, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);

        device.TryGetTelegram(true, out byte[] payload).Should().BeTrue();

        payload[5].Should().Be(AppIdentifier1);
        payload[6].Should().Be(AppIdentifier2);
    }

    /// <summary>
    /// This test checks that the payload identifiers are correctly set in the connect datagram for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(new byte[] { 0xab, 0xcd })]
    [InlineData(new byte[] { 0x12, 0x34 })]
    public void TryGetTelegram_CommandDatagram_PayloadIdentifier(byte[] deviceAddress)
    {
        byte[] scanData = [0x00, 0x00, 0x00, 0x00, 0x00, deviceAddress[0], deviceAddress[1], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
        CaDARaceCar device = new CaDARaceCar("CaDARaceCar", BitConverter.ToString(deviceAddress).ToLower(), scanData, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);

        device.TryGetTelegram(false, out byte[] payload).Should().BeTrue();

        payload[0].Should().Be(PayloadCommandIdentifier1);
        payload[1].Should().Be(PayloadIdentifier2);
        payload[12].Should().Be(PayloadCommandFooter[0]);
        payload[13].Should().Be(PayloadCommandFooter[1]);
        payload[14].Should().Be(PayloadCommandFooter[2]);
        payload[15].Should().Be(PayloadCommandFooter[3]);
    }

    /// <summary>
    /// This test checks that the AppIdentifier is correctly included in the connect datagram payload for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(new byte[] { 0xab, 0xcd })]
    [InlineData(new byte[] { 0x12, 0x34 })]
    public void TryGetTelegram_CommandDatagram_AppIdentifier(byte[] deviceAddress)
    {
        byte[] scanData = [0x00, 0x00, 0x00, 0x00, 0x00, deviceAddress[0], deviceAddress[1], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
        CaDARaceCar device = new CaDARaceCar("CaDARaceCar", BitConverter.ToString(deviceAddress).ToLower(), scanData, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);

        device.TryGetTelegram(false, out byte[] payload).Should().BeTrue();

        payload[5].Should().Be(AppIdentifier1);
        payload[6].Should().Be(AppIdentifier2);
    }

    /// <summary>
    /// Tests that setting an illegal channel index throws an ArgumentOutOfRangeException.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(new byte[] { 0xab, 0xcd })]
    [InlineData(new byte[] { 0x12, 0x34 })]
    public void TryGetTelegram_CommandDatagram_SetIllegalChannel(byte[] deviceAddress)
    {
        byte[] scanData = [0x00, 0x00, 0x00, 0x00, 0x00, deviceAddress[0], deviceAddress[1], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
        CaDARaceCar device = new CaDARaceCar("CaDARaceCar", BitConverter.ToString(deviceAddress).ToLower(), scanData, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);

        Action action = () => device.SetOutput(device.NumberOfChannels, 0);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// This test checks that the command datagram payload is correctly constructed based on the set output values for multiple devices, ensuring that each device's payload is independent of the others.
    /// </summary>
    [Fact]
    public void TryGetTelegram_CommandDatagram_InstanceInteraction()
    {
        byte[] deviceAddress1 = [0xab, 0xcd];
        byte[] scanData1 = [0x00, 0x00, 0x00, 0x00, 0x00, deviceAddress1[0], deviceAddress1[1], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        CaDARaceCar device1 = new CaDARaceCar("CaDARaceCar1", BitConverter.ToString(deviceAddress1).ToLower(), scanData1, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);

        float[] setValues1 = [1.0f, 1.0f, 0.0f];
        byte[] expectedPayload1 = [
            PayloadCommandIdentifier1, PayloadIdentifier2,
            0x11, // product/model identifier (CaDA RaceCar)
            deviceAddress1[0], deviceAddress1[1],
            AppIdentifier1, AppIdentifier2,
            0xde, 0x21, 0x00, // throttle, steering, lights
            0xde, 0x01, // checksum, sequence
            PayloadCommandFooter[0], PayloadCommandFooter[1], PayloadCommandFooter[2], PayloadCommandFooter[3]]; // 4 bytes footer

        byte[] deviceAddress2 = [0x12, 0x34];
        byte[] scanData2 = [0x00, 0x00, 0x00, 0x00, 0x00, deviceAddress2[0], deviceAddress2[1], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        CaDARaceCar device2 = new CaDARaceCar("CaDARaceCar2", BitConverter.ToString(deviceAddress2).ToLower(), scanData2, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);
        float[] setValues2 = [-1.0f, -1.0f, 0.0f];
        byte[] expectedPayload2 = [
            PayloadCommandIdentifier1, PayloadIdentifier2,
            0x11, // product/model identifier (CaDA RaceCar)
            deviceAddress2[0], deviceAddress2[1],
            AppIdentifier1, AppIdentifier2,
            0x53, 0xac, 0x00, // throttle, steering, lights
            0xac, 0x01, // checksum, sequence
            PayloadCommandFooter[0], PayloadCommandFooter[1], PayloadCommandFooter[2], PayloadCommandFooter[3]]; // 4 bytes footer

        byte[] deviceAddress3 = [0x78, 0x90];
        byte[] scanData3 = [0x00, 0x00, 0x00, 0x00, 0x00, deviceAddress3[0], deviceAddress3[1], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        CaDARaceCar device3 = new CaDARaceCar("CaDARaceCar3", BitConverter.ToString(deviceAddress3).ToLower(), scanData3, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);
        float[] setValues3 = [1.0f, -1.0f, 1.0f];
        byte[] expectedPayload3 = [
            PayloadCommandIdentifier1, PayloadIdentifier2,
            0x11, // product/model identifier (CaDA RaceCar)
            deviceAddress3[0], deviceAddress3[1],
            AppIdentifier1, AppIdentifier2,
            0x70, 0x70, 0x01, // throttle, steering, lights
            0x70, 0x01, // checksum, sequence
            PayloadCommandFooter[0], PayloadCommandFooter[1], PayloadCommandFooter[2], PayloadCommandFooter[3]]; // 4 bytes footer

        // Set the output values for the device
        for (int i = 0; i < device1.NumberOfChannels; i++)
        {
            device1.SetOutput(i, setValues1[i]);
            device2.SetOutput(i, setValues2[i]);
            device3.SetOutput(i, setValues3[i]);
        }

        // Get the command datagram payload
        device1.TryGetTelegram(false, out byte[] payload1).Should().BeTrue();
        device2.TryGetTelegram(false, out byte[] payload2).Should().BeTrue();
        device3.TryGetTelegram(false, out byte[] payload3).Should().BeTrue();

        // Check that the payload matches the expected values
        payload1.Should().BeEquivalentTo(expectedPayload1);
        payload2.Should().BeEquivalentTo(expectedPayload2);
        payload3.Should().BeEquivalentTo(expectedPayload3);
    }

    /// <summary>
    /// This test checks that the command datagram payload is correctly constructed based on the set output values for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    /// <param name="setValues">The set values for each channel.</param>
    /// <param name="expectedPayload">The expected payload for the command datagram.</param>
    [Theory]
    [InlineData(new byte[] { 0xab, 0xcd }, new float[] { 0.0f, 0.0f, 0.0f }, new byte[] { PayloadCommandIdentifier1, PayloadIdentifier2, 0x11, 0x00, 0x00, AppIdentifier1, AppIdentifier2, 0x5e, 0x5e, 0x00, 0xde, 0x00, 0x00, 0x00, 0x00, 0x00 })]      // all channels neutral
    [InlineData(new byte[] { 0xcd, 0xef }, new float[] { 1.0f, 0.0f, 0.0f }, new byte[] { PayloadCommandIdentifier1, PayloadIdentifier2, 0x11, 0x00, 0x00, AppIdentifier1, AppIdentifier2, 0xa3, 0x23, 0x00, 0xa3, 0x01, 0x00, 0x00, 0x00, 0x00 })]      // channel 1 maximum, others neutral
    [InlineData(new byte[] { 0xef, 0xab }, new float[] { 0.0f, 1.0f, 0.0f }, new byte[] { PayloadCommandIdentifier1, PayloadIdentifier2, 0x11, 0x00, 0x00, AppIdentifier1, AppIdentifier2, 0x00, 0x7f, 0x00, 0x80, 0x01, 0x00, 0x00, 0x00, 0x00 })]      // channel 2 maximum, others neutral
    [InlineData(new byte[] { 0x11, 0x22 }, new float[] { 0.0f, 0.0f, 1.0f }, new byte[] { PayloadCommandIdentifier1, PayloadIdentifier2, 0x11, 0x00, 0x00, AppIdentifier1, AppIdentifier2, 0x1a, 0x1a, 0x01, 0x9a, 0x00, 0x00, 0x00, 0x00, 0x00 })]      // channel 3 maximum, others neutral
    [InlineData(new byte[] { 0x22, 0x33 }, new float[] { -1.0f, 0.0f, 0.0f }, new byte[] { PayloadCommandIdentifier1, PayloadIdentifier2, 0x11, 0x00, 0x00, AppIdentifier1, AppIdentifier2, 0xc4, 0xbb, 0x00, 0x3b, 0x01, 0x00, 0x00, 0x00, 0x00 })]     // channel 1 minimum, others neutral
    [InlineData(new byte[] { 0x33, 0x44 }, new float[] { 0.0f, -1.0f, 0.0f }, new byte[] { PayloadCommandIdentifier1, PayloadIdentifier2, 0x11, 0x00, 0x00, AppIdentifier1, AppIdentifier2, 0xde, 0x5e, 0x00, 0x5e, 0x01, 0x00, 0x00, 0x00, 0x00 })]     // channel 2 minimum, others neutral
    [InlineData(new byte[] { 0x44, 0x55 }, new float[] { 0.0f, 0.0f, -1.0f }, new byte[] { PayloadCommandIdentifier1, PayloadIdentifier2, 0x11, 0x00, 0x00, AppIdentifier1, AppIdentifier2, 0x80, 0x80, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })]     // channel 3 minimum, others neutral
    [InlineData(new byte[] { 0x55, 0x66 }, new float[] { 9.0f, 9.0f, 9.0f }, new byte[] { PayloadCommandIdentifier1, PayloadIdentifier2, 0x11, 0x00, 0x00, AppIdentifier1, AppIdentifier2, 0x22, 0xdd, 0x01, 0x22, 0x01, 0x00, 0x00, 0x00, 0x00 })]     // all channels above maximum, should be clamped to maximum
    [InlineData(new byte[] { 0x66, 0x77 }, new float[] { -9.0f, -9.0f, -9.0f }, new byte[] { PayloadCommandIdentifier1, PayloadIdentifier2, 0x11, 0x00, 0x00, AppIdentifier1, AppIdentifier2, 0xbb, 0x44, 0x01, 0x44, 0x01, 0x00, 0x00, 0x00, 0x00 })]  // all channels below minimum, should be clamped to minimum
    public void TryGetTelegram_CommandDatagram_Payload(byte[] deviceAddress, float[] setValues, byte[] expectedPayload)
    {
        byte[] scanData = [0x00, 0x00, 0x00, 0x00, 0x00, deviceAddress[0], deviceAddress[1], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
        CaDARaceCar device = new CaDARaceCar("CaDARaceCar", BitConverter.ToString(deviceAddress).ToLower(), scanData, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);

        deviceAddress.CopyTo(expectedPayload, 3); // Copy device address to expectedPayload at index 3
        PayloadCommandFooter.CopyTo(expectedPayload, 12); // Copy footer to expectedPayload at index 12)

        // Set the output values for the device
        for (int i = 0; i < device.NumberOfChannels; i++)
        {
            device.SetOutput(i, setValues[i]);
        }

        // Get the command datagram payload
        device.TryGetTelegram(false, out byte[] payload).Should().BeTrue();

        // Check that the payload matches the expected values
        payload.Should().BeEquivalentTo(expectedPayload);
    }
}
