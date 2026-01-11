using BrickController2.Settings;
using FluentAssertions;
using Microsoft.Maui.Graphics;
using Xunit;

namespace BrickController2.Tests.Settings;

public class RgbColorTests
{
    [Theory]
    [InlineData(0xFF0000, 1f, 0f, 0f)] // Red
    [InlineData(0x00FF00, 0f, 1f, 0f)] // Green
    [InlineData(0x0000FF, 0f, 0f, 1f)] // Blue
    [InlineData(0xFFFFFF, 1f, 1f, 1f)] // White
    [InlineData(0x000000, 0f, 0f, 0f)] // Black
    [InlineData(0x808080, 0.502f, 0.502f, 0.502f)] // Gray (128/255 ≈ 0.502)
    public void Constructor_FromInt_ConvertsCorrectly(int colorValue, float expectedR, float expectedG, float expectedB)
    {
        var color = new RgbColor(colorValue);

        color.R.Should().BeApproximately(expectedR, 0.001f);
        color.G.Should().BeApproximately(expectedG, 0.001f);
        color.B.Should().BeApproximately(expectedB, 0.001f);
    }

    [Theory]
    [InlineData(1f, 0f, 0f)]
    [InlineData(0f, 1f, 0f)]
    [InlineData(0f, 0f, 1f)]
    [InlineData(0.5f, 0.5f, 0.5f)]
    public void Constructor_FromFloats_StoresCorrectly(float r, float g, float b)
    {
        var color = new RgbColor(r, g, b);

        color.R.Should().Be(r);
        color.G.Should().Be(g);
        color.B.Should().Be(b);
    }

    [Theory]
    [InlineData(0xFF0000, 0xFF0000)] // Red
    [InlineData(0x00FF00, 0x00FF00)] // Green
    [InlineData(0x0000FF, 0x0000FF)] // Blue
    [InlineData(0xFFFFFF, 0xFFFFFF)] // White
    [InlineData(0x000000, 0x000000)] // Black
    [InlineData(0x123456, 0x123456)] // Random color
    public void ToInt_ReturnsOriginalValue(int originalValue, int expectedValue)
    {
        var color = new RgbColor(originalValue);
        var result = color.ToInt();

        result.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(1f, 0f, 0f, 0f, 1f, 1f)] // Red: H=0, S=1, V=1
    [InlineData(0f, 1f, 0f, 0.333f, 1f, 1f)] // Green: H≈0.333, S=1, V=1
    [InlineData(0f, 0f, 1f, 0.667f, 1f, 1f)] // Blue: H≈0.667, S=1, V=1
    [InlineData(1f, 1f, 1f, 0f, 0f, 1f)] // White: H=0, S=0, V=1
    [InlineData(0f, 0f, 0f, 0f, 0f, 0f)] // Black: H=0, S=0, V=0
    [InlineData(0.5f, 0.5f, 0.5f, 0f, 0f, 0.5f)] // Gray: H=0, S=0, V=0.5
    public void ToHsv_ConvertsCorrectly(float r, float g, float b, float expectedH, float expectedS, float expectedV)
    {
        var color = new RgbColor(r, g, b);
        color.ToHsv(out var h, out var s, out var v);

        h.Should().BeApproximately(expectedH, 0.01f);
        s.Should().BeApproximately(expectedS, 0.01f);
        v.Should().BeApproximately(expectedV, 0.01f);
    }

    [Theory]
    [InlineData(0f, 1f, 1f, 1f, 0f, 0f)] // Red
    [InlineData(0.333f, 1f, 1f, 0f, 1f, 0f)] // Green
    [InlineData(0.667f, 1f, 1f, 0f, 0f, 1f)] // Blue
    [InlineData(0f, 0f, 1f, 1f, 1f, 1f)] // White
    [InlineData(0f, 0f, 0f, 0f, 0f, 0f)] // Black
    [InlineData(0.5f, 0.5f, 0.5f, 0.25f, 0.5f, 0.5f)] // Cyan-ish mid-tone
    public void FromHsv_ConvertsCorrectly(float h, float s, float v, float expectedR, float expectedG, float expectedB)
    {
        var color = RgbColor.FromHsv(h, s, v);
        color.R.Should().BeApproximately(expectedR, 0.01f);
        color.G.Should().BeApproximately(expectedG, 0.01f);
        color.B.Should().BeApproximately(expectedB, 0.01f);
    }

    [Theory]
    [InlineData(1f, 0f, 0f, 0.5f, 0.5f, 0f, 0f)] // Red dimmed 50%
    [InlineData(1f, 0f, 0f, 2f, 1f, 0f, 0f)] // Red brightened (clamped to 1.0)
    [InlineData(0f, 1f, 0f, 0.5f, 0f, 0.5f, 0f)] // Green dimmed 50%
    [InlineData(0.5f, 0.5f, 0.5f, 2f, 1f, 1f, 1f)] // Gray brightened (clamped)
    [InlineData(0.5f, 0.5f, 0.5f, 0f, 0f, 0f, 0f)] // Gray to black
    public void WithValueFactor_AdjustsBrightness(float r, float g, float b, float factor, float expectedR, float expectedG, float expectedB)
    {
        var color = new RgbColor(r, g, b);
        var adjusted = color.WithValueFactor(factor);

        adjusted.R.Should().BeApproximately(expectedR, 0.01f);
        adjusted.G.Should().BeApproximately(expectedG, 0.01f);
        adjusted.B.Should().BeApproximately(expectedB, 0.01f);
    }

    [Fact]
    public void ToHsv_ThenFromHsv_RoundTrip_PreservesColor()
    {
        var original = new RgbColor(0.7f, 0.3f, 0.9f);
        original.ToHsv(out var h, out var s, out var v);
        var roundTrip = RgbColor.FromHsv(h, s, v);

        roundTrip.R.Should().BeApproximately(original.R, 0.01f);
        roundTrip.G.Should().BeApproximately(original.G, 0.01f);
        roundTrip.B.Should().BeApproximately(original.B, 0.01f);
    }
}
