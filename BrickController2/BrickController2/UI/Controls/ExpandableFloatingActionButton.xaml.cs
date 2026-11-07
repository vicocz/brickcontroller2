using BrickController2.Helpers;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace BrickController2.UI.Controls;

[ContentProperty(nameof(SecondaryButtons))]
public partial class ExpandableFloatingActionButton : ContentView
{
    private bool _isMenuOpen = false;
    private int _animationToken;

    public ExpandableFloatingActionButton()
    {
        InitializeComponent();

        SyncSecondaryContainer();

        SecondaryButtons.CollectionChanged += OnSecondaryButtonsChanged;

        ApplyState();
    }

    public ObservableCollection<IView> SecondaryButtons { get; } = [];

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(ExpandableFloatingActionButton), "menu", propertyChanged: OnAppearanceChanged);

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly BindableProperty IconColorProperty =
        BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(ExpandableFloatingActionButton), null, propertyChanged: OnAppearanceChanged);

    public Color IconColor
    {
        get => (Color)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public static readonly BindableProperty IconBackgroundColorProperty =
        BindableProperty.Create(nameof(IconBackgroundColor), typeof(Color), typeof(ExpandableFloatingActionButton), null, propertyChanged: OnAppearanceChanged);

    public Color? IconBackgroundColor
    {
        get => (Color?)GetValue(IconBackgroundColorProperty);
        set => SetValue(IconBackgroundColorProperty, value);
    }

    public static readonly BindableProperty IsExpandedProperty =
        BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(ExpandableFloatingActionButton), false, propertyChanged: OnAppearanceChanged);

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        private set => SetValue(IsExpandedProperty, value);
    }

    public static readonly BindableProperty TooltipKeyProperty =
        BindableProperty.Create(nameof(TooltipKey), typeof(string), typeof(ExpandableFloatingActionButton), null, propertyChanged: OnAppearanceChanged);

    /// <summary>
    /// Translation resource key resolved at apply-time. Use this instead of <see cref="Tooltip"/>
    /// when the value comes from a style in App.xaml, because markup extensions there are
    /// evaluated once at startup and would not reflect the selected language.
    /// </summary>
    public string? TooltipKey
    {
        get => (string?)GetValue(TooltipKeyProperty);
        set => SetValue(TooltipKeyProperty, value);
    }

    private static void OnAppearanceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ExpandableFloatingActionButton fab)
        {
            fab.ApplyState();
        }
    }

    private void ApplyState()
    {
        ImageSource.Glyph = Icon;
        SetColor(ImageSource, FontImageSource.ColorProperty, IconColor);
        SetColor(Button, BackgroundColorProperty, IconBackgroundColor);

        var tooltip = string.IsNullOrEmpty(TooltipKey) ? "" : TranslationHelper.Translate(TooltipKey);
        ToolTipProperties.SetText(Button, tooltip);
    }

    private static void SetColor(BindableObject target, BindableProperty property, Color? color)
    {
        if (color is null)
        {
            // No override provided, fall back to whatever the applied style defines
            target.ClearValue(property);
        }
        else
        {
            target.SetValue(property, color);
        }
    }

    private void OnFabClicked(object sender, EventArgs e)
    {
        _isMenuOpen = !_isMenuOpen;

        if (_isMenuOpen)
        {
            IsExpanded = true;
        }

        AnimateMenu();
    }

    private void OnSecondaryButtonsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SyncSecondaryContainer();
    }

    private void SyncSecondaryContainer()
    {
        SecondaryContainer.Children.Clear();

        foreach (var view in SecondaryButtons)
        {
            SecondaryContainer.Children.Add(view);
        }
    }

    private void OnOverlayTapped(object sender, EventArgs e)
    {
        if (_isMenuOpen)
        {
            _isMenuOpen = false;
            AnimateMenu();
        }
    }

    private async void AnimateMenu()
    {
        var token = ++_animationToken;

        if (_isMenuOpen)
        {
            // Grow to fill the parent so the dismiss overlay can cover the whole page
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;

            Overlay.IsVisible = true;
            SecondaryContainer.IsVisible = true;

            await Task.WhenAll(
                SecondaryContainer.FadeToAsync(1, 500, Easing.CubicOut),
                SecondaryContainer.TranslateToAsync(0, 0, 500, Easing.CubicOut),
                Button.RotateToAsync(45, 500, Easing.CubicOut)
            );
        }
        else
        {
            await Task.WhenAll(
                SecondaryContainer.FadeToAsync(0, 500, Easing.CubicIn),
                SecondaryContainer.TranslateToAsync(20, 0, 500, Easing.CubicIn),
                Button.RotateToAsync(0, 500, Easing.CubicIn)
            );

            if (token != _animationToken || _isMenuOpen)
            {
                return;
            }

            Overlay.IsVisible = false;
            SecondaryContainer.IsVisible = false;
            IsExpanded = false;

            // Shrink back to the button footprint so the list underneath stays interactive
            HorizontalOptions = LayoutOptions.End;
            VerticalOptions = LayoutOptions.End;
        }
    }
}