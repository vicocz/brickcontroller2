using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace BrickController2.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FloatingActionButton : ImageButton
    {
        public FloatingActionButton()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(FloatingActionButton), null, BindingMode.OneWay, null, IconChanged);
        public static readonly BindableProperty ImageColorProperty = BindableProperty.Create(nameof(ImageColor), typeof(Color), typeof(FloatingActionButton), null, BindingMode.OneWay, null, ImageColorChanged);

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public Color ImageColor
        {
            get => (Color)GetValue(ImageColorProperty);
            set => SetValue(ImageColorProperty, value);
        }

        private static void IconChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is FloatingActionButton fab && newValue is string iconName)
            {
                fab.ImageSource.Glyph = iconName;
            }
        }

        private static void ImageColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is FloatingActionButton fab && newValue is Color imageColor)
            {
                fab.ImageSource.Color = imageColor;
            }
        }
    }
}