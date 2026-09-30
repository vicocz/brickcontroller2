using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.CaDA;
using BrickController2.PlatformServices.BluetoothLE;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.CaDA;

public class CaDARaceCarTests
{
    /// <summary>
    /// This class is a test implementation of the IMessageEncoderFactory interface that creates TestMessageEncoder as IMessageEncoder.
    /// </summary>
    private class TestMessageEncoderFactory : IMessageEncoderFactory
    {
        public IMessageEncoder Create(ReadOnlySpan<byte> deviceData)
        {
            return new TestMessageEncoder();
        }
    }

    /// <summary>
    /// This class is a test implementation of the IMessageEncoder interface that simulates the behavior of the Encode method for testing purposes.
    /// </summary>
    private class TestMessageEncoder : IMessageEncoder
    {
        public byte[] Encode(ReadOnlySpan<Half> values, bool connectDevice = false) => [(byte)values[0], (byte)values[1], (byte)values[2]];

        public void Initialize() { }
    }

    private readonly Mock<IBluetoothLEService> _bluetoothLEService = new(MockBehavior.Strict);
    private readonly Mock<IDeviceRepository> _deviceRepository = new(MockBehavior.Strict);
    private readonly IMessageEncoderFactory _messageEncoderFactory = new TestMessageEncoderFactory();

    [Theory]
    // all zero
    [InlineData(new float[] { 0.0f, 0.0f, 0.0f, 0.0f }, new byte[] { 0x00, 0x00, 0b0000 })]
    // throttle
    [InlineData(new float[] { 9.0f, 0.0f, 0.0f, 0.0f }, new byte[] { 0x01, 0x00, 0b0000 })]
    [InlineData(new float[] { -9.0f, 0.0f, 0.0f, 0.0f }, new byte[] { 0xff, 0x00, 0b0000 })]
    // steering
    [InlineData(new float[] { 0.0f, 5.0f, 0.0f, 0.0f }, new byte[] { 0x00, 0x01, 0b0000 })]
    [InlineData(new float[] { 0.0f, -5.0f, 0.0f, 0.0f }, new byte[] { 0x00, 0xff, 0b0000 })]
    // lights
    [InlineData(new float[] { 0.0f, 0.0f, 0.5f, 0.0f }, new byte[] { 0x00, 0x00, 0b0000 })]
    [InlineData(new float[] { 0.0f, 0.0f, 0.6f, 0.0f }, new byte[] { 0x00, 0x00, 0b0001 })]
    [InlineData(new float[] { 0.0f, 0.0f, 0.0f, -0.5f }, new byte[] { 0x00, 0x00, 0b0000 })]
    [InlineData(new float[] { 0.0f, 0.0f, 0.0f, -0.6f }, new byte[] { 0x00, 0x00, 0b0010 })]
    [InlineData(new float[] { 0.0f, 0.0f, 9.0f, -0.6f }, new byte[] { 0x00, 0x00, 0b0011 })]
    // all set
    [InlineData(new float[] { 1.0f, 1.0f, 1.0f, 1.0f }, new byte[] { 0x01, 0x01, 0b0011 })]
    public void SetOutput_WithAllChannelsSet_ReturnProperFakePayload(float[] channelValues, byte[] bytes)
    {
        CaDARaceCar device = Create();

        // Set the output values for the device
        for (int i = 0; i < device.NumberOfChannels; i++)
        {
            device.SetOutput(i, channelValues[i]);
        }

        device.TryGetTelegram(true, out byte[] payload).Should().BeTrue();
        payload.Length.Should().Be(3);
        payload[0].Should().Be(bytes[0]);
        payload[1].Should().Be(bytes[1]);
        payload[2].Should().Be(bytes[2]);
    }

    [Theory]
    [InlineData(true, 2, 0b0000, 0b0001)]
    [InlineData(false, 2, 0b0011, 0b0010)]
    [InlineData(true, 3, 0b0000, 0b0010)]
    [InlineData(false, 3, 0b0011, 0b0001)]
    public void SetOutput_ToggleLights_ReturnProperFakePayload(bool toggleOnOff, int channel, byte value1, byte value2)
    {
        CaDARaceCar device = Create();

        // set all values to initial value
        for (int i = 0; i < device.NumberOfChannels; i++)
        {
            device.SetOutput(i, toggleOnOff ? 0.0f : 1.0f);
        }
        byte throttle = toggleOnOff ? (byte)0x00 : (byte)0x01;
        byte steering = toggleOnOff ? (byte)0x00 : (byte)0x01;

        device.TryGetTelegram(true, out byte[] payload).Should().BeTrue();
        payload.Length.Should().Be(3);
        payload[0].Should().Be(throttle);
        payload[1].Should().Be(steering);
        payload[2].Should().Be(value1);

        // do the light toggle
        device.SetOutput(channel, toggleOnOff ? 1.0f : 0.0f); // front light on

        device.TryGetTelegram(true, out payload).Should().BeTrue();
        payload.Length.Should().Be(3);
        payload[0].Should().Be(throttle);
        payload[1].Should().Be(steering);
        payload[2].Should().Be(value2);
    }

    private CaDARaceCar Create()
    {
        return Create([0x12, 0x34, 0x56]);
    }
    private CaDARaceCar Create(byte[] deviceAddress)
    {
        return new CaDARaceCar("CaDARaceCar", BitConverter.ToString(deviceAddress).ToLower(), [], _deviceRepository.Object, _bluetoothLEService.Object, _messageEncoderFactory);
    }
}
