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

        foreach (var view in SecondaryButtons)
        {
            SecondaryContainer.Children.Add(view);
        }

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

    public static readonly BindableProperty TooltipProperty =
        BindableProperty.Create(nameof(Tooltip), typeof(string), typeof(ExpandableFloatingActionButton), null, propertyChanged: OnAppearanceChanged);

    public string? Tooltip
    {
        get => (string?)GetValue(TooltipProperty);
        set => SetValue(TooltipProperty, value);
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

    private static void OnAppearanceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ExpandableFloatingActionButton fab)
        {
            fab.ApplyState();
        }
    }

    private void ApplyState()
    {
        imageSource.Glyph = Icon;
        SetColor(imageSource, FontImageSource.ColorProperty, IconColor);
        SetColor(icon, VisualElement.BackgroundColorProperty, IconBackgroundColor);
        ToolTipProperties.SetText(icon, Tooltip ?? "");
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
        if (e.NewItems != null)
        {
            foreach (IView view in e.NewItems)
            {
                SecondaryContainer.Children.Add(view);
            }
        }

        if (e.OldItems != null)
        {
            foreach (IView view in e.OldItems)
            {
                SecondaryContainer.Children.Remove(view);
            }
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
            // Make elements physically present before animating
            Overlay.IsVisible = true;
            SecondaryContainer.IsVisible = true;

            await Task.WhenAll(
                SecondaryContainer.FadeToAsync(1, 500, Easing.CubicOut),
                SecondaryContainer.TranslateToAsync(0, 0, 500, Easing.CubicOut),
                icon.RotateToAsync(45, 500, Easing.CubicOut)
            );
        }
        else
        {
            // Run closing animations first, keeping the icon glyph unchanged so the
            // rotation doesn't visually snap partway through
            await Task.WhenAll(
                SecondaryContainer.FadeToAsync(0, 500, Easing.CubicIn),
                SecondaryContainer.TranslateToAsync(20, 0, 500, Easing.CubicIn),
                icon.RotateToAsync(0, 500, Easing.CubicIn)
            );

            // If a newer toggle started while this closing animation was running, or the
            // menu has since been reopened, don't clobber the now-current state.
            if (token != _animationToken || _isMenuOpen)
            {
                return;
            }

            // Hide elements and swap the icon back only after the animation finishes
            Overlay.IsVisible = false;
            SecondaryContainer.IsVisible = false;
            IsExpanded = false;
        }
    }
}