using BrickController2.DeviceManagement.CaDA;
using FluentAssertions;
using System;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.CaDA;

public sealed class CaDARaceCarRev1DatagramTests : CaDADatagramTestsBase
{
    private const byte PayloadIdentifier1 = 0x75;
    private const byte PayloadIdentifier2 = 0x13;
    private const byte MockedRandomValue1 = 0xf4;
    private const byte MockedRandomValue2 = 0xf4;

    /// <summary>
    /// This test checks that the payload identifiers are correctly set in the connect datagram for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(new byte[] { 0xab, 0xcd, 0xef })]
    [InlineData(new byte[] { 0x12, 0x34, 0x56 })]
    public void TryGetTelegram_ConnectDatagram_PayloadIdentifier(byte[] deviceAddress)
    {
        byte[] scanData = [0x00, 0x00, 0x00, 0x00, deviceAddress[0], deviceAddress[1], deviceAddress[2], AppIdentifier1, AppIdentifier2, AppIdentifier3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        CaDARaceCar device = new CaDARaceCar("CaDARaceCar", BitConverter.ToString(deviceAddress).ToLower(), scanData, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);

        device.TryGetTelegram(true, out byte[] payload).Should().BeTrue();
        payload[0].Should().Be(PayloadIdentifier1);
        payload[1].Should().Be(PayloadIdentifier2);
    }

    /// <summary>
    /// This test checks that the AppIdentifier is correctly included in the connect datagram payload for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(new byte[] { 0xab, 0xcd, 0xef })]
    [InlineData(new byte[] { 0x12, 0x34, 0x35 })]
    public void TryGetTelegram_ConnectDatagram_AppIdentifier(byte[] deviceAddress)
    {
        byte[] scanData = [0x00, 0x00, 0x00, 0x00, deviceAddress[0], deviceAddress[1], deviceAddress[2], AppIdentifier1, AppIdentifier2, AppIdentifier3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        CaDARaceCar device = new CaDARaceCar("CaDARaceCar", BitConverter.ToString(deviceAddress).ToLower(), scanData, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);

        device.TryGetTelegram(true, out byte[] payload).Should().BeTrue();
        payload[5].Should().Be(AppIdentifier1);
        payload[6].Should().Be(AppIdentifier2);
        payload[7].Should().Be(AppIdentifier3);
    }

    /// <summary>
    /// This test checks that the payload identifiers are correctly set in the connect datagram for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(new byte[] { 0xab, 0xcd, 0xef })]
    [InlineData(new byte[] { 0x12, 0x34, 0x35 })]
    public void TryGetTelegram_CommandDatagram_PayloadIdentifier(byte[] deviceAddress)
    {
        byte[] scanData = [0x00, 0x00, 0x00, 0x00, deviceAddress[0], deviceAddress[1], deviceAddress[2], AppIdentifier1, AppIdentifier2, AppIdentifier3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        CaDARaceCar device = new CaDARaceCar("CaDARaceCar", BitConverter.ToString(deviceAddress).ToLower(), scanData, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);

        device.TryGetTelegram(false, out byte[] payload).Should().BeTrue();
        payload[0].Should().Be(PayloadIdentifier1);
        payload[1].Should().Be(PayloadIdentifier2);
    }

    /// <summary>
    /// This test checks that the AppIdentifier is correctly included in the connect datagram payload for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(new byte[] { 0xab, 0xcd, 0xef })]
    [InlineData(new byte[] { 0x12, 0x34, 0x35 })]
    public void TryGetTelegram_CommandDatagram_AppIdentifier(byte[] deviceAddress)
    {
        byte[] scanData = [0x00, 0x00, 0x00, 0x00, deviceAddress[0], deviceAddress[1], deviceAddress[2], AppIdentifier1, AppIdentifier2, AppIdentifier3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        CaDARaceCar device = new CaDARaceCar("CaDARaceCar", BitConverter.ToString(deviceAddress).ToLower(), scanData, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);

        device.TryGetTelegram(false, out byte[] payload).Should().BeTrue();
        payload[5].Should().Be(AppIdentifier1);
        payload[6].Should().Be(AppIdentifier2);
        payload[7].Should().Be(AppIdentifier3);
    }

    /// <summary>
    /// Tests that setting an illegal channel index throws an ArgumentOutOfRangeException.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(new byte[] { 0xab, 0xcd, 0xef })]
    [InlineData(new byte[] { 0x12, 0x34, 0x35 })]
    public void TryGetTelegram_CommandDatagram_SetIllegalChannel(byte[] deviceAddress)
    {
        byte[] scanData = [0x00, 0x00, 0x00, 0x00, deviceAddress[0], deviceAddress[1], deviceAddress[2], AppIdentifier1, AppIdentifier2, AppIdentifier3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

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
        byte[] deviceAddress1 = [0xab, 0xcd, 0xef];
        byte[] scanData1 = [0x00, 0x00, 0x00, 0x00, deviceAddress1[0], deviceAddress1[1], deviceAddress1[2], AppIdentifier1, AppIdentifier2, AppIdentifier3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        CaDARaceCar device1 = new CaDARaceCar("CaDARaceCar1", BitConverter.ToString(deviceAddress1).ToLower(), scanData1, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);

        float[] setValues1 = [1.0f, 1.0f, 0.0f];
        byte[] expectedPayload1 = [
            PayloadIdentifier1, PayloadIdentifier2,
            deviceAddress1[0], deviceAddress1[1], deviceAddress1[2],
            AppIdentifier1, AppIdentifier2, AppIdentifier3,
            MockedRandomValue1, MockedRandomValue2, // mocked random values
            0xc9, 0x1a, 0x21, 0xc9, 0xc9, 0xc9];

        byte[] deviceAddress2 = [0x12, 0x34, 0x56];
        byte[] scanData2 = [0x00, 0x00, 0x00, 0x00, deviceAddress2[0], deviceAddress2[1], deviceAddress2[2], AppIdentifier1, AppIdentifier2, AppIdentifier3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        CaDARaceCar device2 = new CaDARaceCar("CaDARaceCar2", BitConverter.ToString(deviceAddress2).ToLower(), scanData2, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);

        float[] setValues2 = [-1.0f, -1.0f, 0.0f];
        byte[] expectedPayload2 = [
            PayloadIdentifier1, PayloadIdentifier2,
            deviceAddress2[0], deviceAddress2[1], deviceAddress2[2],
            AppIdentifier1, AppIdentifier2, AppIdentifier3,
            MockedRandomValue1, MockedRandomValue2, // mocked random values
            0x1a, 0xc9, 0x21, 0xc9, 0xc9, 0xc9];

        byte[] deviceAddress3 = [0x78, 0x90, 0xab];
        byte[] scanData3 = [0x00, 0x00, 0x00, 0x00, deviceAddress3[0], deviceAddress3[1], deviceAddress3[2], AppIdentifier1, AppIdentifier2, AppIdentifier3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        CaDARaceCar device3 = new CaDARaceCar("CaDARaceCar3", BitConverter.ToString(deviceAddress3).ToLower(), scanData3, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);

        float[] setValues3 = [1.0f, -1.0f, 1.0f];
        byte[] expectedPayload3 = [
            PayloadIdentifier1, PayloadIdentifier2,
            deviceAddress3[0], deviceAddress3[1], deviceAddress3[2],
            AppIdentifier1, AppIdentifier2, AppIdentifier3,
            MockedRandomValue1, MockedRandomValue2, // mocked random values
            0xc9, 0xc9, 0x1a, 0xc9, 0xc9, 0xc9];

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
    [InlineData(new byte[] { 0xab, 0xcd, 0xef }, new float[] { 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifier1, PayloadIdentifier2, 0x00, 0x00, 0x00, AppIdentifier1, AppIdentifier2, AppIdentifier3, MockedRandomValue1, MockedRandomValue2, 0x21, 0x21, 0x21, 0xc9, 0xc9, 0xc9 })]      // all channels neutral
    [InlineData(new byte[] { 0xcd, 0xef, 0xab }, new float[] { 1.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifier1, PayloadIdentifier2, 0x00, 0x00, 0x00, AppIdentifier1, AppIdentifier2, AppIdentifier3, MockedRandomValue1, MockedRandomValue2, 0xc9, 0x21, 0x21, 0xc9, 0xc9, 0xc9 })]      // channel 1 maximum, others neutral
    [InlineData(new byte[] { 0xef, 0xab, 0xcd }, new float[] { 0.0f, 1.0f, 0.0f }, new byte[] { PayloadIdentifier1, PayloadIdentifier2, 0x00, 0x00, 0x00, AppIdentifier1, AppIdentifier2, AppIdentifier3, MockedRandomValue1, MockedRandomValue2, 0x21, 0x1a, 0x21, 0xc9, 0xc9, 0xc9 })]      // channel 2 maximum, others neutral
    [InlineData(new byte[] { 0x11, 0x22, 0x33 }, new float[] { 0.0f, 0.0f, 1.0f }, new byte[] { PayloadIdentifier1, PayloadIdentifier2, 0x00, 0x00, 0x00, AppIdentifier1, AppIdentifier2, AppIdentifier3, MockedRandomValue1, MockedRandomValue2, 0x21, 0x21, 0x1a, 0xc9, 0xc9, 0xc9 })]      // channel 3 maximum, others neutral
    [InlineData(new byte[] { 0x22, 0x33, 0x44 }, new float[] { -1.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifier1, PayloadIdentifier2, 0x00, 0x00, 0x00, AppIdentifier1, AppIdentifier2, AppIdentifier3, MockedRandomValue1, MockedRandomValue2, 0x1a, 0x21, 0x21, 0xc9, 0xc9, 0xc9 })]     // channel 1 minimum, others neutral
    [InlineData(new byte[] { 0x33, 0x44, 0x55 }, new float[] { 0.0f, -1.0f, 0.0f }, new byte[] { PayloadIdentifier1, PayloadIdentifier2, 0x00, 0x00, 0x00, AppIdentifier1, AppIdentifier2, AppIdentifier3, MockedRandomValue1, MockedRandomValue2, 0x21, 0xc9, 0x21, 0xc9, 0xc9, 0xc9 })]     // channel 2 minimum, others neutral
    [InlineData(new byte[] { 0x44, 0x55, 0x66 }, new float[] { 0.0f, 0.0f, -1.0f }, new byte[] { PayloadIdentifier1, PayloadIdentifier2, 0x00, 0x00, 0x00, AppIdentifier1, AppIdentifier2, AppIdentifier3, MockedRandomValue1, MockedRandomValue2, 0x21, 0x21, 0xc9, 0xc9, 0xc9, 0xc9 })]     // channel 3 minimum, others neutral
    [InlineData(new byte[] { 0x55, 0x66, 0x77 }, new float[] { 9.0f, 9.0f, 9.0f }, new byte[] { PayloadIdentifier1, PayloadIdentifier2, 0x00, 0x00, 0x00, AppIdentifier1, AppIdentifier2, AppIdentifier3, MockedRandomValue1, MockedRandomValue2, 0xc9, 0x1a, 0x1a, 0xc9, 0xc9, 0xc9 })]     // all channels above maximum, should be clamped to maximum
    [InlineData(new byte[] { 0x66, 0x77, 0x88 }, new float[] { -9.0f, -9.0f, -9.0f }, new byte[] { PayloadIdentifier1, PayloadIdentifier2, 0x00, 0x00, 0x00, AppIdentifier1, AppIdentifier2, AppIdentifier3, MockedRandomValue1, MockedRandomValue2, 0x1a, 0xc9, 0xc9, 0xc9, 0xc9, 0xc9 })]  // all channels below minimum, should be clamped to minimum
    public void TryGetTelegram_CommandDatagram_Payload(byte[] deviceAddress, float[] setValues, byte[] expectedPayload)
    {
        byte[] scanData = [0x00, 0x00, 0x00, 0x00, deviceAddress[0], deviceAddress[1], deviceAddress[2], AppIdentifier1, AppIdentifier2, AppIdentifier3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
        CaDARaceCar device = new CaDARaceCar("CaDARaceCar", BitConverter.ToString(deviceAddress).ToLower(), scanData, _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);

        deviceAddress.CopyTo(expectedPayload, 2); // Copy device address to expectedPayload at index 2

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
