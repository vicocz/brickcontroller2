using BrickController2.DeviceManagement.CaDA;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.CaDA;

public class MessageEncoderFactoryTests
{
    private readonly Mock<ICaDADeviceManager> _cadaManager = new(MockBehavior.Strict);
    private readonly Mock<Random> _random = new(MockBehavior.Strict);
    private readonly MessageEncoderFactory _factory;

    public MessageEncoderFactoryTests()
    {
        _cadaManager.Setup(x => x.GetAppId()).Returns(new byte[] { 0x13, 0x57, 0x9B });
        _factory = new MessageEncoderFactory(_cadaManager.Object, new PlatformService.Default(), _random.Object);
    }

    [Fact]
    public void Create_WithLength16DeviceData_ReturnsRaceCarMessageEncoderRev2()
    {
        // Arrange
        byte[] deviceData =
        [
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x20, 0xB9, // deviceId bytes (little-endian: 0xB920)
            0x00, 0x00, 0x00, 0x00,
            0x55, // sequence byte at index 11
            0x00, 0x00, 0x00, 0x00
        ];

        // Act
        var result = _factory.Create(deviceData);

        // Assert
        result.Should().NotBeNull()
            .And.BeOfType<RaceCarMessageEncoderRev2>();
    }

    [Fact]
    public void Create_WithLength16DeviceData_UsesCorrectDeviceIdAndSequence()
    {
        // Arrange
        byte[] deviceData =
        [
            0x00, 0x00, 0x00, 0x00, 0x00,
            0xC9, 0xC1, // deviceId bytes (little-endian: 0xC1C9)
            0x00, 0x00, 0x00, 0x00,
            0xAB, // sequence byte
            0x00, 0x00, 0x00, 0x00
        ];

        // Act
        var result = _factory.Create(deviceData);

        // Assert
        result.Should().NotBeNull()
            .And.BeOfType<RaceCarMessageEncoderRev2>();

        var encoded = result!.Encode([Half.Zero, Half.Zero, Half.Zero], true);
        encoded.Length.Should().Be(16);
        // Verify deviceId is present in output (bytes 3-4)
        encoded[3].Should().Be(0xC9);
        encoded[4].Should().Be(0xC1);
        // Verify appId is present in output (bytes 5-6)
        encoded[5].Should().Be(0x13);
        encoded[6].Should().Be(0x57);
    }

    [Fact]
    public void Create_WithLength18DeviceData_ReturnsRaceCarMessageEncoder()
    {
        // Arrange
        byte[] deviceData = new byte[18];

        // Act
        var result = _factory.Create(deviceData);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RaceCarMessageEncoder>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(17)]
    [InlineData(19)]
    [InlineData(32)]
    public void Create_WithUnsupportedLength_ThrowsInvalidOperationException(int length)
    {
        // Arrange
        byte[] deviceData = new byte[length];

        // Act
        Action act = () => _factory.Create(deviceData);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Unsupported device data format.");
    }
}