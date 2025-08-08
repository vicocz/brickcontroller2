using FluentAssertions;
using System.Linq;
using Xunit;
using BrickController2.Protocols;
using BrickController2.Tools.Protocols;

namespace BrickController2.Tests.Protocols;

public class CaDAProtocolTests
{
    [Theory]
    [InlineData(new byte[] { 0xaa, 0xbb, 0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde },
                new byte[] { 0x8a, 0xfb, 0x50, 0xfa, 0xdd, 0x73, 0x44, 0xcb, 0xde })]
    public void CaDAProtocol_Encrypt_ShouldReturnExpectedByteArray(byte[] input, byte[] expected)
    {
        byte[] copy = input.ToArray(); // Ensure we work with a copy
        CaDAProtocol.Encrypt(copy);

        copy.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(new byte[] { 0x8a, 0xfb, 0x50, 0xfa, 0xdd, 0x73, 0x44, 0xcb, 0xde },
                new byte[] { 0xaa, 0xbb, 0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde })]
    public void CaDAProtocol_Decrypt_ShouldReturnExpectedByteArray(byte[] input, byte[] expected)
    {
        byte[] copy = input.ToArray(); // Ensure we work with a copy
        CaDATools.Decrypt(copy);

        copy.Should().BeEquivalentTo(expected);
    }
}
