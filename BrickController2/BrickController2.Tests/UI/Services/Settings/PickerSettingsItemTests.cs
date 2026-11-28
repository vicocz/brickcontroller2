// Unit tests for the delegate based picker row of the settings page.
using BrickController2.UI.Services.Settings;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BrickController2.Tests.UI.Services.Settings;

public class PickerSettingsItemTests
{
    [Fact]
    public void Value_ReflectsDelegate()
    {
        var current = "A";
        var item = new PickerSettingsItem("Title", () => current, () => Task.CompletedTask);

        item.Title.Should().Be("Title");
        item.Value.Should().Be("A");
        current = "B";
        item.Value.Should().Be("B");
    }

    [Fact]
    public void RaiseValueChanged_RaisesPropertyChangedForValue()
    {
        var item = new PickerSettingsItem("Title", () => "A", () => Task.CompletedTask);
        var raised = new List<string?>();
        item.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        item.RaiseValueChanged();

        raised.Should().Equal(nameof(PickerSettingsItem.Value));
    }

    [Fact]
    public void Command_ExecutesDelegate()
    {
        var executed = 0;
        var item = new PickerSettingsItem("Title", () => "A", () => { executed++; return Task.CompletedTask; });

        item.Command.Execute(null);

        executed.Should().Be(1);
    }

    [Fact]
    public void Defaults_AreVisibleWithEmptyStatusAndNoOpLifecycle()
    {
        ISettingsItem item = new PickerSettingsItem("Title", () => "A", () => Task.CompletedTask);

        item.IsVisible.Should().BeTrue();
        item.Status.Should().BeEmpty();
        item.Invoking(i => { i.Attach(); i.Detach(); }).Should().NotThrow();
    }
}
