using BrickController2.DeviceManagement.IO;
using FluentAssertions;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.IO;

public class StateStoreTests
{
    private readonly StateStore<string, int> _sut = new(initialState: 0);

    [Fact]
    public void Get_ReturnsDefault_WhenKeyNotPresent()
    {
        _sut.Get("missing").Should().Be(0);
    }

    [Fact]
    public void Set_AndGet_ReturnStoredValue()
    {
        _sut.Set("key", 42);
        _sut.Get("key").Should().Be(42);
    }

    [Fact]
    public void Count_ReflectsNumberOfKeys()
    {
        _sut.Set("a", 1);
        _sut.Set("b", 2);
        _sut.Count.Should().Be(2);
    }

    [Fact]
    public void Remove_ReturnsTrueAndRemovesKey()
    {
        _sut.Set("key", 10);
        _sut.Remove("key").Should().BeTrue();
        _sut.Get("key").Should().Be(0);
    }

    [Fact]
    public void Remove_ReturnsFalse_WhenKeyNotPresent()
    {
        _sut.Remove("ghost").Should().BeFalse();
    }

    [Fact]
    public void Update_AppliesUpdaterToExistingValue()
    {
        _sut.Set("key", 5);
        var result = _sut.Update("key", v => v + 3);
        result.Should().Be(8);
        _sut.Get("key").Should().Be(8);
    }

    [Fact]
    public void Update_UsesDefaultWhenKeyNotPresent()
    {
        var result = _sut.Update("new", v => v + 7);
        result.Should().Be(7);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        _sut.Set("a", 1);
        _sut.Set("b", 2);
        _sut.Clear();
        _sut.Count.Should().Be(0);
    }

    [Fact]
    public void Max_ReturnsMaxProjectedValue()
    {
        _sut.Set("a", 3);
        _sut.Set("b", 7);
        _sut.Set("c", 1);
        _sut.Max(v => v).Should().Be(7);
    }

    [Fact]
    public void Max_ReturnsDefault_WhenEmpty()
    {
        _sut.Max(v => v).Should().Be(0);
    }
}