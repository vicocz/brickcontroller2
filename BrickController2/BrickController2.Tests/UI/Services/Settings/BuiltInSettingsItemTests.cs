// Unit tests of the built-in settings rows: theme, language and orientation sensor.
using BrickController2.InputDeviceManagement.Sensors;
using BrickController2.PlatformServices.InputDeviceService;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Localization;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Settings;
using BrickController2.UI.Services.Theme;
using BrickController2.UI.Services.Translation;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BrickController2.Tests.UI.Services.Settings;

public class BuiltInSettingsItemTests
{
    private readonly Mock<ITranslationService> _translation = new();
    private readonly Mock<IDialogService> _dialog = new();

    public BuiltInSettingsItemTests()
    {
        // the stub returns the key so titles are easy to assert
        _translation.Setup(t => t.Translate(It.IsAny<string>())).Returns((string key) => key);
    }

    /// <summary>
    /// SafeCommand runs the async delegate fire-and-forget, so wait until the work finished.
    /// </summary>
    private static async Task PressAsync(ISettingsItem item)
    {
        item.Command.Execute(null);
        await Task.Delay(150, TestContext.Current.CancellationToken);
    }

    [Fact]
    public void Orders_PlaceBuiltInRowsBeforeDefaultPlatformRows()
    {
        var theme = new ThemeSettingsItem(Mock.Of<IThemeService>(), _dialog.Object, _translation.Object);
        var language = new LanguageSettingsItem(Mock.Of<ILocalizationService>(), _dialog.Object, Mock.Of<INavigationService>(), _translation.Object, () => { });
        var orientation = new OrientationSensorSettingsItem(Mock.Of<IInputDeviceService<OrientationSensorController>>(), _translation.Object);

        new[] { theme.Order, language.Order, orientation.Order }.Should().BeInAscendingOrder().And.OnlyContain(o => o < 100);
        theme.Title.Should().Be("Theme");
        language.Title.Should().Be("Language");
        orientation.Title.Should().Be("OrientationSensor");
    }

    [Fact]
    public async Task ThemeItem_SelectingThemeUpdatesServiceAndRaisesValueChanged()
    {
        var current = ThemeType.Light;
        var theme = new Mock<IThemeService>();
        theme.SetupGet(t => t.CurrentTheme).Returns(() => current);
        theme.SetupSet(t => t.CurrentTheme = It.IsAny<ThemeType>()).Callback<ThemeType>(v => current = v);
        _dialog.Setup(d => d.ShowSelectionDialogAsync(It.IsAny<IEnumerable<string>>(), "Theme", "Cancel", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SelectionDialogResult<string>(true, ThemeType.Dark.ToString()));
        var item = new ThemeSettingsItem(theme.Object, _dialog.Object, _translation.Object);
        var valueChanged = false;
        item.PropertyChanged += (_, e) => valueChanged |= e.PropertyName == nameof(ISettingsItem.Value);

        await PressAsync(item);

        current.Should().Be(ThemeType.Dark);
        item.Value.Should().Be(nameof(ThemeType.Dark));
        valueChanged.Should().BeTrue();
    }

    [Fact]
    public async Task ThemeItem_CancelledDialogChangesNothing()
    {
        var theme = new Mock<IThemeService>();
        _dialog.Setup(d => d.ShowSelectionDialogAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SelectionDialogResult<string>(false, string.Empty));

        await PressAsync(new ThemeSettingsItem(theme.Object, _dialog.Object, _translation.Object));

        theme.VerifySet(t => t.CurrentTheme = It.IsAny<ThemeType>(), Times.Never);
    }

    [Fact]
    public async Task ThemeItem_DetachCancelsTheDialogToken()
    {
        CancellationToken captured = default;
        _dialog.Setup(d => d.ShowSelectionDialogAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<string>, string, string, CancellationToken>((_, _, _, token) => captured = token)
            .ReturnsAsync(new SelectionDialogResult<string>(false, string.Empty));
        var item = new ThemeSettingsItem(Mock.Of<IThemeService>(), _dialog.Object, _translation.Object);

        item.Attach();
        await PressAsync(item);
        captured.IsCancellationRequested.Should().BeFalse();

        item.Detach();
        captured.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public async Task LanguageItem_SelectingLanguageAppliesReloadsAndNavigatesBack()
    {
        var calls = new List<string>();
        var current = Language.System;
        var localization = new Mock<ILocalizationService>();
        localization.SetupGet(l => l.CurrentLanguage).Returns(() => current);
        localization.SetupSet(l => l.CurrentLanguage = It.IsAny<Language>()).Callback<Language>(v => { current = v; calls.Add("set"); });
        var navigation = new Mock<INavigationService>();
        navigation.Setup(n => n.NavigateBackAsync()).Callback(() => calls.Add("back")).Returns(Task.CompletedTask);
        var language = Enum.GetNames<Language>()[^1];
        _dialog.Setup(d => d.ShowSelectionDialogAsync(It.IsAny<IEnumerable<string>>(), "Language", "Cancel", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SelectionDialogResult<string>(true, language));
        _dialog.Setup(d => d.ShowProgressDialogAsync(false, It.IsAny<Func<IProgressDialog, CancellationToken, Task>>(), "Applying", It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns<bool, Func<IProgressDialog, CancellationToken, Task>, string, string?, string?, CancellationToken>(async (_, action, _, _, _, token) =>
            {
                await action(Mock.Of<IProgressDialog>(), token);
                return new ProgressDialogResult(false);
            });
        var item = new LanguageSettingsItem(localization.Object, _dialog.Object, navigation.Object, _translation.Object, () => calls.Add("reload"));

        await Task.Run(async () => { item.Command.Execute(null); await Task.Delay(800, TestContext.Current.CancellationToken); }, TestContext.Current.CancellationToken);

        calls.Should().Equal("set", "reload", "back");
        item.Value.Should().Be(language);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OrientationItem_VisibilityFollowsSupport(bool supported)
    {
        var sensor = new Mock<IInputDeviceService<OrientationSensorController>>();
        sensor.SetupGet(s => s.IsSupported).Returns(supported);

        new OrientationSensorSettingsItem(sensor.Object, _translation.Object).IsVisible.Should().Be(supported);
    }

    [Fact]
    public void OrientationItem_ToggleWritesServiceState()
    {
        var sensor = new Mock<IInputDeviceService<OrientationSensorController>>();
        sensor.SetupProperty(s => s.IsEnabled);
        var item = new OrientationSensorSettingsItem(sensor.Object, _translation.Object);

        item.IsToggled = true;

        sensor.Object.IsEnabled.Should().BeTrue();
    }
}
