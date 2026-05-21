using BrickController2.DeviceManagement.IO;
using FluentAssertions;
using System.Threading.Tasks;
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

    [Fact]
    public void Exchange_ReturnsOldState_AndAppliesUpdater()
    {
        var store = new StateStore<string, TestState>();
        store.Set("key", new TestState(10, true));

        var old = store.Exchange("key", s => s with { Value = s.Value + 5, Updated = false });

        old.Should().BeEquivalentTo(new TestState(10, true));
        var current = store.Get("key");
        current.Should().BeEquivalentTo(new TestState(15));
    }

    [Fact]
    public void Exchange_WhenKeyAbsent_ReturnsDefault_AndSetsNext()
    {
        var store = new StateStore<string, TestState>();

        var old = store.Exchange("missing", s => new TestState(s.Value + 1, true));

        old.Should().BeEquivalentTo(new TestState());
        var current = store.Get("missing");
        current.Should().BeEquivalentTo(new TestState(1, true));
    }

    [Fact]
    public void Exchange_IsSafeUnderConcurrency()
    {
        var store = new StateStore<string, int>();
        store.Set("counter", 0);

        const int iterations = 250;
        Parallel.For(0, iterations, _ =>
        {
            store.Exchange("counter", v => v + 1);
        });

        Assert.Equal(iterations, store.Get("counter"));
    }

    private record struct TestState(int Value = default, bool Updated = false);
}