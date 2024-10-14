using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace BrickController2.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ColorImage : Image
    {
        public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(ColorImage), null, BindingMode.OneWay, null, IconChanged);
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(ColorImage), default(Color), BindingMode.OneWay, null, ColorChanged);
        public ColorImage()
        {
            InitializeComponent();
        }
        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }
        private static void IconChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ColorImage image && newValue is string iconName)
            {
                image.ImageSource.Glyph = iconName;
            }
        }
        private static void ColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ColorImage image && newValue is Color color)
            {
                image.ImageSource.Color = color;
            }
        }
    }
}