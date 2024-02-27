using System.Windows.Input;

namespace BrickController2.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImageButton : ContentView
    {
        public ImageButton()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(ToolbarIcon), null, BindingMode.OneWay, null, IconChanged);
        public static readonly BindableProperty ImageColorProperty = BindableProperty.Create(nameof(ImageColor), typeof(Color), typeof(ImageButton), default(Color), BindingMode.OneWay, null, ImageColorChanged);
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ImageButton), null, BindingMode.OneWay, null, CommandChanged);
        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(Command), typeof(object), typeof(ImageButton), null, BindingMode.OneWay, null, CommandParameterChanged);

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

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        private static void IconChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ImageButton imageButton && newValue is string iconName)
            {
                imageButton.ImageSource.Glyph = iconName;
            }
        }

        private static void ImageColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ImageButton imageButton && newValue is Color color)
            {
                imageButton.ImageSource.Color = color;
            }
        }

        private static void CommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ImageButton imageButton && newValue is ICommand command)
            {
                imageButton.TapGuestureRecognizer.Command = command;
            }
        }

        private static void CommandParameterChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ImageButton imageButton)
            {
                imageButton.TapGuestureRecognizer.CommandParameter = newValue;
            }
        }
    }
}