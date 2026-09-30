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

    [Theory]
    [InlineData(50)]
    [InlineData(50L)]
    [InlineData(50.0)]
    [InlineData(50f)]
    public void ValueEquals_FloatReference_MatchesSavedNumericOfAnyType(object saved)
    {
        MacroChoiceValue reference = 50f;

        Assert.True(reference.ValueEquals(new MacroChoiceValue(saved)));
    }

    [Fact]
    public void ValueEquals_FloatReference_ComparesWithFloatPrecision()
    {
        MacroChoiceValue reference = 0.1f;

        Assert.True(reference.ValueEquals(new MacroChoiceValue(0.1d)));
    }

    [Theory]
    [InlineData(50)]
    [InlineData(50L)]
    public void ValueEquals_IntReference_MatchesSavedIntegral(object saved)
    {
        MacroChoiceValue reference = 50;

        Assert.True(reference.ValueEquals(new MacroChoiceValue(saved)));
    }

    [Theory]
    [InlineData(50.0)]
    [InlineData(50.5)]
    [InlineData(50f)]
    public void ValueEquals_IntReference_RejectsSavedFloatingPoint(object saved)
    {
        MacroChoiceValue reference = 50;

        Assert.False(reference.ValueEquals(new MacroChoiceValue(saved)));
    }

    [Fact]
    public void ValueEquals_IntReference_ReturnsFalse_ForIntegralsDifferingByOne()
    {
        MacroChoiceValue reference = 16777216;

        Assert.False(reference.ValueEquals(new MacroChoiceValue(16777217L)));
    }

    [Fact]
    public void ValueEquals_LongReference_ReturnsFalse_WhenSavedUlongOverflows()
    {
        var reference = new MacroChoiceValue(long.MaxValue);

        Assert.False(reference.ValueEquals(new MacroChoiceValue(ulong.MaxValue)));
    }

    [Fact]
    public void ValueEquals_StringReference_RejectsNumeric()
    {
        MacroChoiceValue reference = "50";

        Assert.False(reference.ValueEquals(new MacroChoiceValue(50)));
    }

    [Fact]
    public void ValueEquals_NumericReference_RejectsString()
    {
        MacroChoiceValue reference = 50f;

        Assert.False(reference.ValueEquals("50"));
    }

    [Fact]
    public void ValueEquals_StringReference_IsOrdinal()
    {
        MacroChoiceValue reference = "Sound.mp3";

        Assert.True(reference.ValueEquals("Sound.mp3"));
        Assert.False(reference.ValueEquals("sound.mp3"));
    }

    [Fact]
    public void ValueEquals_BoolReference_MatchesOnlyBool()
    {
        MacroChoiceValue reference = true;

        Assert.True(reference.ValueEquals(true));
        Assert.False(reference.ValueEquals(false));
        Assert.False(reference.ValueEquals(1));
    }

    [Fact]
    public void ValueEquals_HandlesNull()
    {
        Assert.True(default(MacroChoiceValue).ValueEquals(default));
        Assert.False(default(MacroChoiceValue).ValueEquals(1));
        Assert.False(new MacroChoiceValue(1).ValueEquals(default));
    }

    [Fact]
    public void ValueEquals_FloatReference_HandlesNaN()
    {
        MacroChoiceValue reference = float.NaN;

        Assert.True(reference.ValueEquals(new MacroChoiceValue(double.NaN)));
        Assert.False(reference.ValueEquals(new MacroChoiceValue(0L)));
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
