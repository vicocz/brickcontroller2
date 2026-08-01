using BrickController2.DeviceManagement.JieStar;
using FluentAssertions;
using System;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.JieStar;

public sealed class JieStarSCM4DatagramTests : JieStarDatagramTestsBase
{
    private const byte PayloadIdentifierConnect1 = 0xa4;
    private const byte PayloadIdentifierConnect2 = 0x5b;
    private const byte PayloadIdentifierCommand1 = 0x40;
    private const byte PayloadIdentifierCommand2 = 0xbf;

    /// <summary>
    /// This test checks that the payload identifiers are correctly set in the connect datagram for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(JieStarSCM4.Device1)]
    [InlineData(JieStarSCM4.Device2)]
    [InlineData(JieStarSCM4.Device3)]
    public void TryGetTelegram_ConnectDatagram_PayloadIdentifier(string deviceAddress)
    {
        JieStarSCM4 device = new JieStarSCM4("JieStarSCM4", deviceAddress, [], _deviceRepository.Object, _bluetoothLEService.Object, _jieStarPlatformService, _manager.Object);

        device.TryGetTelegram(true, out byte[] payload).Should().BeTrue();
        payload[0].Should().Be(PayloadIdentifierConnect1);
        payload[7].Should().Be(PayloadIdentifierConnect2);
    }

    /// <summary>
    /// This test checks that the AppIdentifier is correctly included in the connect datagram payload for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(JieStarSCM4.Device1)]
    [InlineData(JieStarSCM4.Device2)]
    [InlineData(JieStarSCM4.Device3)]
    public void TryGetTelegram_ConnectDatagram_AppIdentifier(string deviceAddress)
    {
        JieStarSCM4 device = new JieStarSCM4("JieStarSCM4", deviceAddress, [], _deviceRepository.Object, _bluetoothLEService.Object, _jieStarPlatformService, _manager.Object);

        device.TryGetTelegram(true, out byte[] payload).Should().BeTrue();
        payload[1].Should().Be(AppIdentifier1);
        payload[2].Should().Be(AppIdentifier2);
    }

    /// <summary>
    /// This test checks that the payload identifiers are correctly set in the command datagram for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(JieStarSCM4.Device1)]
    [InlineData(JieStarSCM4.Device2)]
    [InlineData(JieStarSCM4.Device3)]
    public void TryGetTelegram_CommandDatagram_PayloadIdentifier(string deviceAddress)
    {
        JieStarSCM4 device = new JieStarSCM4("JieStarSCM4", deviceAddress, [], _deviceRepository.Object, _bluetoothLEService.Object, _jieStarPlatformService, _manager.Object);

        device.TryGetTelegram(false, out byte[] payload).Should().BeTrue();
        payload[0].Should().Be(PayloadIdentifierCommand1);
        payload[7].Should().Be(PayloadIdentifierCommand2);
    }

    /// <summary>
    /// This test checks that the AppIdentifier is correctly included in the command datagram payload for each device address.
    /// </summary>
    /// <param name="deviceAddress">The address of the device to test.</param>
    [Theory]
    [InlineData(JieStarSCM4.Device1)]
    [InlineData(JieStarSCM4.Device2)]
    [InlineData(JieStarSCM4.Device3)]
    public void TryGetTelegram_CommandDatagram_AppIdentifier(string deviceAddress)
    {
        JieStarSCM4 device = new JieStarSCM4("JieStarSCM4", deviceAddress, [], _deviceRepository.Object, _bluetoothLEService.Object, _jieStarPlatformService, _manager.Object);

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
    [InlineData(JieStarSCM4.Device1, new float[] { 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x00, 0x00, 0x80, 0x80, PayloadIdentifierCommand2 })]      // all channels neutral
    [InlineData(JieStarSCM4.Device1, new float[] { 1.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0xf0, 0x00, 0x80, 0x80, PayloadIdentifierCommand2 })]      // channel 1 maximum, others neutral
    [InlineData(JieStarSCM4.Device1, new float[] { 0.0f, 1.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x0f, 0x00, 0x80, 0x80, PayloadIdentifierCommand2 })]      // channel 2 maximum, others neutral
    [InlineData(JieStarSCM4.Device1, new float[] { 0.0f, 0.0f, 1.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x00, 0xf0, 0x80, 0x80, PayloadIdentifierCommand2 })]      // channel 3 maximum, others neutral
    [InlineData(JieStarSCM4.Device1, new float[] { 0.0f, 0.0f, 0.0f, 1.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x00, 0x0f, 0x80, 0x80, PayloadIdentifierCommand2 })]      // channel 4 maximum, others neutral
    [InlineData(JieStarSCM4.Device1, new float[] { -1.0f, 0.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x70, 0x00, 0x80, 0x80, PayloadIdentifierCommand2 })]     // channel 1 minimum, others neutral
    [InlineData(JieStarSCM4.Device1, new float[] { 0.0f, -1.0f, 0.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x07, 0x00, 0x80, 0x80, PayloadIdentifierCommand2 })]     // channel 2 minimum, others neutral
    [InlineData(JieStarSCM4.Device1, new float[] { 0.0f, 0.0f, -1.0f, 0.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x00, 0x70, 0x80, 0x80, PayloadIdentifierCommand2 })]     // channel 3 minimum, others neutral
    [InlineData(JieStarSCM4.Device1, new float[] { 0.0f, 0.0f, 0.0f, -1.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x00, 0x07, 0x80, 0x80, PayloadIdentifierCommand2 })]     // channel 4 minimum, others neutral
    [InlineData(JieStarSCM4.Device1, new float[] { 9.0f, 9.0f, 9.0f, 9.0f, }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0xff, 0xff, 0x80, 0x80, PayloadIdentifierCommand2 })]     // all channels above maximum, should be clamped to maximum
    [InlineData(JieStarSCM4.Device1, new float[] { -9.0f, -9.0f, -9.0f, -9.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x77, 0x77, 0x80, 0x80, PayloadIdentifierCommand2 })]  // all channels below minimum, should be clamped to minimum

    [InlineData(JieStarSCM4.Device2, new float[] { 0.0f, 0.0f, 0.0f, 0.0f }, new byte[]     { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x00, 0x00, 0x80, 0x80, PayloadIdentifierCommand2 })]      // all channels neutral
    [InlineData(JieStarSCM4.Device2, new float[] { 9.0f, 9.0f, 9.0f, 9.0f }, new byte[]     { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0xff, 0xff, 0x80, 0x80, PayloadIdentifierCommand2 })]      // all channels above maximum, should be clamped to maximum
    [InlineData(JieStarSCM4.Device2, new float[] { -9.0f, -9.0f, -9.0f, -9.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x77, 0x77, 0x80, 0x80, PayloadIdentifierCommand2 })]  // all channels below minimum, should be clamped to minimum

    [InlineData(JieStarSCM4.Device3, new float[] { 0.0f, 0.0f, 0.0f, 0.0f }, new byte[]     { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x00, 0x00, 0x80, 0x80, PayloadIdentifierCommand2 })]      // all channels neutral
    [InlineData(JieStarSCM4.Device3, new float[] { 9.0f, 9.0f, 9.0f, 9.0f }, new byte[]     { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0xff, 0xff, 0x80, 0x80, PayloadIdentifierCommand2 })]      // all channels above maximum, should be clamped to maximum
    [InlineData(JieStarSCM4.Device3, new float[] { -9.0f, -9.0f, -9.0f, -9.0f }, new byte[] { PayloadIdentifierCommand1, AppIdentifier1, AppIdentifier2, 0x77, 0x77, 0x80, 0x80, PayloadIdentifierCommand2 })]  // all channels below minimum, should be clamped to minimum
    public void TryGetTelegram_CommandDatagram_Payload(string deviceAddress, float[] setValues, byte[] expectedPayload)
    {
        JieStarSCM4 device = new JieStarSCM4("JieStarSCM4", deviceAddress, [], _deviceRepository.Object, _bluetoothLEService.Object, _jieStarPlatformService, _manager.Object);

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
    [InlineData(JieStarSCM4.Device1)]
    [InlineData(JieStarSCM4.Device2)]
    [InlineData(JieStarSCM4.Device3)]
    public void TryGetTelegram_CommandDatagram_SetIllegalChannel(string deviceAddress)
    {
        JieStarSCM4 device = new JieStarSCM4("JieStarSCM4", deviceAddress, [], _deviceRepository.Object, _bluetoothLEService.Object, _jieStarPlatformService, _manager.Object);

        Action action = () => device.SetOutput(device.NumberOfChannels, 0);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
