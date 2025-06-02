using BrickController2.Protocols;
using FluentAssertions;
using Xunit;

namespace BrickController2.Tests.Protocols;

public class MKProtocolTests
{
    private const int IosHeaderOffset = 13;
    private const int IosPayloadLength = 26;

    [Fact]
    public void GetRfPayload_ConnectTelegramWithIosPayloadSettings_ProperlyEncoded()
    {
        // arrange
        var payload = new byte[IosPayloadLength];
        var telegramConnect = new byte[] { 0xad, 0x3f, 0x4d, 0x80, 0x80, 0x80, 0xb9, 0x52 };

        // act
        int payloadLength = MKProtocol.GetRfPayload(MKProtocol.SeedArray, telegramConnect, IosHeaderOffset, MKProtocol.CTXValue1, MKProtocol.CTXValue2, payload);

        // assert
        payloadLength.Should().Be(18);
        payload.Should().BeEquivalentTo([0xf9, 0x08, 0x49, 0x22, 0x47, 0xba, 0xc4, 0xbc, 0xc3, 0xab, 0xf3, 0x8a, 0x6d, 0xb9, 0x8c, 0xd1,
            0xcb, 0x0b, // crc
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
    }

    [Fact]
    public void GetRfPayload_BaseTelegramWithIosPayloadSettings_ProperlyEncoded()
    {
        // arrange
        var payload = new byte[IosPayloadLength];
        byte[] telegramBase = [0x7d, 0x3f, 0x4d, 0x00, 0x00, 0x80, 0x80, 0x80, 0x80, 0x82];

        // act
        int payloadLength = MKProtocol.GetRfPayload(MKProtocol.SeedArray, telegramBase, IosHeaderOffset, MKProtocol.CTXValue1, MKProtocol.CTXValue2, payload);

        // assert
        payloadLength.Should().Be(20);
        payload.Should().BeEquivalentTo([0xf9, 0x08, 0x49, 0x22, 0x47, 0xba, 0xc4, 0xbc, 0x13, 0xab, 0xf3, 0x0a, 0xed, 0xb9, 0xb5, 0x03, 0x2d, 0x09,
            0xd3, 0xfc, // crc
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
    }
}
