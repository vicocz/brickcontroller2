using BrickController2.PlatformServices.InputDevice;
using Xunit;

namespace BrickController2.Tests.PlatformServices.InputDevice;

public class AndroidBluetoothAliasMatcherTests
{
    // AOSP descriptor fixture for vendor 054c/product 09cc and uniqueId aa:bb:cc:dd:ee:ff.
    private const string Descriptor = "779bbe401e3280862aca5d220574d52751fddba9";

    [Fact]
    public void ExactDescriptorIdentifiesNicknameAmongIdenticalModelNames()
    {
        var name = AndroidBluetoothAliasMatcher.FindAlias(Descriptor, 0x054c, 0x09cc, "Wireless Controller",
            [("AA:BB:CC:DD:EE:FF", "GamePad White"), ("11:22:33:44:55:66", "GamePad Black")]);
        Assert.Equal("GamePad White", name);
    }

    [Fact]
    public void ChangingNicknameDoesNotChangeIdentity()
    {
        Assert.Equal("New nickname", AndroidBluetoothAliasMatcher.FindAlias(Descriptor, 0x054c, 0x09cc,
            "Wireless Controller", [("aa:bb:cc:dd:ee:ff", "New nickname")]));
    }

    [Theory]
    [InlineData(null, "AA:BB:CC:DD:EE:FF", "White")]
    [InlineData("unknown-oem-format", "AA:BB:CC:DD:EE:FF", "White")]
    [InlineData(Descriptor, "XX:XX:XX:XX:EE:FF", "White")]
    [InlineData(Descriptor, "AA:BB:CC:DD:EE:FF", null)]
    [InlineData(Descriptor, "AA:BB:CC:DD:EE:FF", " ")]
    public void UnsupportedOrMissingInformationDoesNotGuess(string? descriptor, string address, string? alias)
    {
        Assert.Null(AndroidBluetoothAliasMatcher.FindAlias(descriptor, 0x054c, 0x09cc,
            "Wireless Controller", [(address, alias)]));
    }

    [Fact]
    public void DifferentVendorOrAmbiguousMatchesDoNotGuess()
    {
        Assert.Null(AndroidBluetoothAliasMatcher.FindAlias(Descriptor, 0x1234, 0x09cc,
            "Wireless Controller", [("AA:BB:CC:DD:EE:FF", "White")]));
        Assert.Null(AndroidBluetoothAliasMatcher.FindAlias(Descriptor, 0x054c, 0x09cc,
            "Wireless Controller", [("AA:BB:CC:DD:EE:FF", "White"), ("AA:BB:CC:DD:EE:FF", "Black")]));
    }
}
