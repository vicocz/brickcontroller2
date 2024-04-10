namespace BrickController2.UI.Controls;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SwipeIcon : SwipeItem
{
    public SwipeIcon()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(SwipeIcon), null, BindingMode.OneWay, null, IconChanged);

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    private static void IconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SwipeIcon icon && newValue is string iconName)
        {
            icon.ImageSource.Glyph = iconName;
        }
    }
}