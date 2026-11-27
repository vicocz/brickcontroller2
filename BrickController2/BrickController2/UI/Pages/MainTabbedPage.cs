using System.Linq;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Graphics;

namespace BrickController2.UI.Pages;

/// <summary>
/// Prototype root page hosting the main sections as native tabs, each with its own navigation stack.
/// </summary>
public class MainTabbedPage : Microsoft.Maui.Controls.TabbedPage
{
    private readonly NavigationState _state;
    private Page? _lastPage;
    private bool _reverting;

    public MainTabbedPage(NavigationState state, params (NavigationPage Page, string Title, string Glyph)[] tabs)
    {
        _state = state;

        On<Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
        BarBackgroundColor = Colors.Red;
        SelectedTabColor = Colors.White;
        UnselectedTabColor = Color.FromArgb("#B3FFFFFF");

        foreach (var (page, title, glyph) in tabs)
        {
            page.Title = title;
            page.IconImageSource = new FontImageSource { FontFamily = "Icons", Glyph = glyph };
            Children.Add(page);
        }

        _lastPage = CurrentPage;
        CurrentPageChanged += OnCurrentPageChanged;
    }

    public NavigationPage? FindTab(string rootKey) => Children
        .OfType<NavigationPage>()
        .FirstOrDefault(p => p.RootPage?.BindingContext is PageViewModelBase vm && vm.RootKey == rootKey);

    private void OnCurrentPageChanged(object? sender, System.EventArgs e)
    {
        if (_reverting)
        {
            return;
        }

        if (!_state.IsNavigationEnabled && _lastPage is not null && CurrentPage != _lastPage)
        {
            // block tab switching while a dialog is open
            _reverting = true;
            CurrentPage = _lastPage;
            _reverting = false;
            return;
        }

        _lastPage = CurrentPage;
        if (CurrentPage is NavigationPage { RootPage.BindingContext: PageViewModelBase { RootKey: { } key } })
        {
            _state.ActiveRootKey = key;
        }
    }
}
