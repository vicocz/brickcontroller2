using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ToolbarIcon : ToolbarItem
{
    public ToolbarIcon()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(ToolbarIcon), null, BindingMode.OneWay, null, IconChanged);

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    private static void IconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ToolbarIcon toolbarIcon && newValue is string iconName)
        {
            toolbarIcon.ImageSource.Glyph = iconName;
        }
    }
}