using BrickController2.DeviceManagement.Macros;
using Newtonsoft.Json;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.Macros;

public class MacroChoiceValueTests
{
    [Fact]
    public void ImplicitConversion_FromInt_SetsValue()
    {
        MacroChoiceValue value = 42;

        Assert.True(value.HasValue);
        Assert.Equal(42, value.Value);
    }

    [Fact]
    public void ImplicitConversion_FromFloat_SetsValue()
    {
        MacroChoiceValue value = 3.5f;

        Assert.True(value.HasValue);
        Assert.Equal(3.5f, value.Value);
    }

    [Fact]
    public void ImplicitConversion_FromBool_SetsValue()
    {
        MacroChoiceValue value = true;

        Assert.True(value.HasValue);
        Assert.Equal(true, value.Value);
    }

    [Fact]
    public void ImplicitConversion_FromString_SetsValue()
    {
        MacroChoiceValue value = "sound.wav";

        Assert.True(value.HasValue);
        Assert.Equal("sound.wav", value.Value);
    }

    [Fact]
    public void HasValue_IsFalse_WhenDefault()
    {
        MacroChoiceValue value = default;

        Assert.False(value.HasValue);
    }

    [Fact]
    public void TryGet_ReturnsTrue_ForExactType()
    {
        MacroChoiceValue value = "sound.wav";

        var success = value.TryGet<string>(out var result);

        Assert.True(success);
        Assert.Equal("sound.wav", result);
    }

    [Fact]
    public void TryGet_ReturnsTrue_WhenLongWidensToInt()
    {
        var value = new MacroChoiceValue(42L);

        var success = value.TryGet<int>(out var result);

        Assert.True(success);
        Assert.Equal(42, result);
    }

    [Fact]
    public void TryGet_ReturnsTrue_WhenDoubleWidensToFloat()
    {
        var value = new MacroChoiceValue(3.5d);

        var success = value.TryGet<float>(out var result);

        Assert.True(success);
        Assert.Equal(3.5f, result);
    }

    [Fact]
    public void TryGet_ReturnsFalse_WhenIncompatible()
    {
        var value = new MacroChoiceValue("not a number");

        var success = value.TryGet<int>(out _);

        Assert.False(success);
    }

    [Fact]
    public void As_ReturnsDefault_WhenIncompatible()
    {
        var value = new MacroChoiceValue("not a number");

        var result = value.As<int>();

        Assert.Equal(0, result);
    }

    [Fact]
    public void Equality_MatchesBoxedValue()
    {
        MacroChoiceValue a = "sound.wav";
        MacroChoiceValue b = "sound.wav";

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsUnderlyingValueToString()
    {
        MacroChoiceValue value = 42;

        Assert.Equal("42", value.ToString());
    }

    [Fact]
    public void JsonSerialization_RoundTrips_AsWrapperObject()
    {
        MacroChoiceValue value = "sound.wav";

        var json = JsonConvert.SerializeObject(value);
        var deserialized = JsonConvert.DeserializeObject<MacroChoiceValue>(json);

        Assert.Contains("\"Value\"", json);
        Assert.Equal(value, deserialized);
    }
}
