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
    public void TryGet_Enum_ReturnsTrue_ForBoxedEnum()
    {
        var value = new MacroChoiceValue(TestEnum.Second);

        Assert.True(value.TryGet<TestEnum>(out var result));
        Assert.Equal(TestEnum.Second, result);
    }

    [Theory]
    [InlineData("First", TestEnum.First)]
    [InlineData("second", TestEnum.Second)]
    [InlineData("SECOND", TestEnum.Second)]
    [InlineData("2", TestEnum.Second)]
    public void TryGet_Enum_ParsesString_IgnoringCase(string text, TestEnum expected)
    {
        MacroChoiceValue value = text;

        Assert.True(value.TryGet<TestEnum>(out var result));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("")]
    [InlineData("42")]
    public void TryGet_Enum_ReturnsFalse_ForInvalidString(string text)
    {
        MacroChoiceValue value = text;

        Assert.False(value.TryGet<TestEnum>(out var result));
        Assert.Equal(default, result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1L)]
    [InlineData((short)1)]
    [InlineData((byte)1)]
    public void TryGet_Enum_ConvertsIntegral(object saved)
    {
        var value = new MacroChoiceValue(saved);

        Assert.True(value.TryGet<TestEnum>(out var result));
        Assert.Equal(TestEnum.First, result);
    }

    [Theory]
    [InlineData(99)]
    [InlineData(-1)]
    [InlineData(99L)]
    public void TryGet_Enum_ReturnsFalse_ForUndefinedIntegral(object saved)
    {
        var value = new MacroChoiceValue(saved);

        Assert.False(value.TryGet<TestEnum>(out _));
    }

    [Fact]
    public void TryGet_Enum_ReturnsFalse_ForOtherEnumType()
    {
        var value = new MacroChoiceValue(OtherEnum.One);

        Assert.False(value.TryGet<TestEnum>(out _));
    }

    [Fact]
    public void TryGet_Enum_ReturnsFalse_ForNullOrNonEnumLike()
    {
        Assert.False(default(MacroChoiceValue).TryGet<TestEnum>(out _));
        Assert.False(new MacroChoiceValue(1.5f).TryGet<TestEnum>(out _));
        Assert.False(new MacroChoiceValue(true).TryGet<TestEnum>(out _));
    }

    [Fact]
    public void As_Enum_ReturnsParsedValue_OrDefault()
    {
        Assert.Equal(TestEnum.First, new MacroChoiceValue("First").As<TestEnum>());
        Assert.Equal(TestEnum.None, new MacroChoiceValue("Nope").As<TestEnum>());
    }

    [Theory]
    [InlineData(TestEnum.First)]
    [InlineData("First")]
    [InlineData("first")]
    [InlineData(1)]
    [InlineData(1L)]
    public void ValueEquals_EnumReference_MatchesSavedRepresentations(object saved)
    {
        var reference = new MacroChoiceValue(TestEnum.First);

        Assert.True(reference.ValueEquals(new MacroChoiceValue(saved)));
    }

    [Theory]
    [InlineData(TestEnum.Second)]
    [InlineData("Second")]
    [InlineData("Unknown")]
    [InlineData(2)]
    [InlineData(2L)]
    [InlineData(1.0f)]
    [InlineData(true)]
    public void ValueEquals_EnumReference_RejectsDifferentValues(object saved)
    {
        var reference = new MacroChoiceValue(TestEnum.First);

        Assert.False(reference.ValueEquals(new MacroChoiceValue(saved)));
    }

    [Fact]
    public void ValueEquals_EnumReference_RejectsOtherEnumType()
    {
        var reference = new MacroChoiceValue(TestEnum.First);

        Assert.False(reference.ValueEquals(new MacroChoiceValue(OtherEnum.One)));
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

    public enum TestEnum
    {
        None = 0,
        First = 1,
        Second = 2
    }

    public enum OtherEnum
    {
        Zero = 0,
        One = 1
    }
}
