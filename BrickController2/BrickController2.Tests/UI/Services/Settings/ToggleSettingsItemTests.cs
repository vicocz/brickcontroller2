// Unit tests for the delegate based toggle row of the settings page.
using BrickController2.UI.Services.Settings;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace BrickController2.Tests.UI.Services.Settings;

public class ToggleSettingsItemTests
{
    [Fact]
    public void IsToggled_ReadsAndWritesThroughDelegates()
    {
        var state = false;
        var item = new ToggleSettingsItem("Title", () => true, () => state, v => state = v);

        item.IsToggled.Should().BeFalse();
        item.IsToggled = true;

        state.Should().BeTrue();
        item.IsToggled.Should().BeTrue();
    }

    [Fact]
    public void IsToggled_Setter_RaisesPropertyChanged()
    {
        var state = false;
        var item = new ToggleSettingsItem("Title", () => true, () => state, v => state = v);
        var raised = new List<string?>();
        item.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        item.IsToggled = true;
        item.IsToggled = true; // unchanged value: no second notification

        raised.Should().Equal(nameof(ToggleSettingsItem.IsToggled));
    }

    [Fact]
    public void IsVisible_FollowsDelegate()
    {
        var visible = false;
        var item = new ToggleSettingsItem("Title", () => visible, () => false, _ => { });

        item.IsVisible.Should().BeFalse();
        visible = true;
        item.IsVisible.Should().BeTrue();
    }

    [Fact]
    public void ImplementsToggleInterface_WithUnusedMembersEmpty()
    {
        var item = new ToggleSettingsItem("Title", () => true, () => false, _ => { });

        item.Should().BeAssignableTo<ISettingsToggleItem>();
        item.Value.Should().BeEmpty();
        item.Status.Should().BeEmpty();
    }
}
