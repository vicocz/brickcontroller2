// Unit tests for the settings page view model: it only orders, attaches and detaches the injected rows.
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Settings;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.ViewModels;
using FluentAssertions;
using Moq;
using System.Linq;
using Xunit;

namespace BrickController2.Tests.UI.ViewModels;

public class SettingsPageViewModelTests
{
    private static SettingsPageViewModel CreateViewModel(params ISettingsItem[] items)
        => new(Mock.Of<INavigationService>(), Mock.Of<ITranslationService>(), items);

    private static Mock<ISettingsItem> Item(string title, int? order = null)
    {
        var item = new Mock<ISettingsItem>();
        item.SetupGet(i => i.Title).Returns(title);
        // Moq does not call default interface members, so the default of 100 is set up explicitly
        item.SetupGet(i => i.Order).Returns(order ?? 100);
        return item;
    }

    [Fact]
    public void SettingsItems_AreSortedByOrderRegardlessOfRegistrationOrder()
    {
        // platform row registered first (platform modules are loaded before the UI module)
        var viewModel = CreateViewModel(Item("Platform").Object, Item("Orientation", 20).Object, Item("Theme", 0).Object, Item("Language", 10).Object);

        viewModel.SettingsItems.Select(i => i.Title).Should().Equal("Theme", "Language", "Orientation", "Platform");
    }

    [Fact]
    public void SettingsItems_WithEqualOrder_KeepRegistrationOrder()
    {
        var viewModel = CreateViewModel(Item("A").Object, Item("B").Object);

        viewModel.SettingsItems.Select(i => i.Title).Should().Equal("A", "B");
    }

    [Fact]
    public void SettingsItems_WithoutItems_IsEmpty()
    {
        CreateViewModel().SettingsItems.Should().BeEmpty();
    }

    [Fact]
    public void OnAppearingAndDisappearing_AttachAndDetachAllItems()
    {
        var first = Item("A");
        var second = Item("B");
        var viewModel = CreateViewModel(first.Object, second.Object);

        viewModel.OnAppearing();
        first.Verify(i => i.Attach(), Times.Once);
        second.Verify(i => i.Attach(), Times.Once);
        first.Verify(i => i.Detach(), Times.Never);

        viewModel.OnDisappearing();
        first.Verify(i => i.Detach(), Times.Once);
        second.Verify(i => i.Detach(), Times.Once);
    }
}
