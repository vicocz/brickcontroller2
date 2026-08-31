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

    public ExpandableFloatingActionButton()
    {
        InitializeComponent();

        SecondaryButtons.CollectionChanged += OnSecondaryButtonsChanged;
    }

    public ObservableCollection<IView> SecondaryButtons { get; } = [];

    public static readonly BindableProperty FabIconProperty =
        BindableProperty.Create(nameof(FabIcon), typeof(string), typeof(ExpandableFloatingActionButton), "+");

    public string FabIcon
    {
        get => (string)GetValue(FabIconProperty);
        set => SetValue(FabIconProperty, value);
    }

    public static readonly BindableProperty FabColorProperty =
        BindableProperty.Create(nameof(FabColor), typeof(Color), typeof(ExpandableFloatingActionButton), Colors.Blue);

    public Color FabColor
    {
        get => (Color)GetValue(FabColorProperty);
        set => SetValue(FabColorProperty, value);
    }

    private void OnFabClicked(object sender, EventArgs e)
    {
        _isMenuOpen = !_isMenuOpen;
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
        if (_isMenuOpen)
        {
            // Make elements physically present before animating
            Overlay.IsVisible = true;
            SecondaryContainer.IsVisible = true;

            await Task.WhenAll(
                SecondaryContainer.FadeToAsync(1, 250, Easing.CubicOut),
                SecondaryContainer.TranslateToAsync(0, 0, 250, Easing.CubicOut),
                Icon.RotateToAsync(45, 250, Easing.CubicOut)
            );
        }
        else
        {
            // Run closing animations
            await Task.WhenAll(
                SecondaryContainer.FadeToAsync(0, 250, Easing.CubicIn),
                SecondaryContainer.TranslateToAsync(20, 0, 250, Easing.CubicIn),
                Icon.RotateToAsync(0, 250, Easing.CubicIn)
            );

            // Hide elements entirely after animation finishes
            Overlay.IsVisible = false;
            SecondaryContainer.IsVisible = false;
        }
    }
}