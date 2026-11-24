using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BottomNavButton : ContentView
    {
        public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(BottomNavButton), null);
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(BottomNavButton), null);
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(BottomNavButton), null);
        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(BottomNavButton), null);
        public static readonly BindableProperty KeyProperty = BindableProperty.Create(nameof(Key), typeof(string), typeof(BottomNavButton), null, propertyChanged: OnKeysChanged);
        public static readonly BindableProperty ActiveKeyProperty = BindableProperty.Create(nameof(ActiveKey), typeof(string), typeof(BottomNavButton), null, propertyChanged: OnKeysChanged);
        public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(BottomNavButton), false);

        public BottomNavButton()
        {
            InitializeComponent();
        }

        public string? Icon
        {
            get => (string?)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string? Text
        {
            get => (string?)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public string? Key
        {
            get => (string?)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public string? ActiveKey
        {
            get => (string?)GetValue(ActiveKeyProperty);
            set => SetValue(ActiveKeyProperty, value);
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            private set => SetValue(IsActiveProperty, value);
        }

        private static void OnKeysChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var button = (BottomNavButton)bindable;
            button.IsActive = button.Key is not null && button.Key == button.ActiveKey;
        }
    }
}
