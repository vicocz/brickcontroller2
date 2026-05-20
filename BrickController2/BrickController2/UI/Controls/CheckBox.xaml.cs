using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CheckBox : ContentView
    {
        public CheckBox()
        {
            InitializeComponent();
            UpdateView();
            TapRecognizer.Command = new Command(() =>
            {
                Checked = !Checked;
            }, canExecute: CanChangeCheckbox);
        }

        public static readonly BindableProperty CheckedProperty = BindableProperty.Create(nameof(Checked), typeof(bool), typeof(CheckBox), false, BindingMode.TwoWay, null, CheckedChanged);
        public static readonly BindableProperty ReadOnlyProperty = BindableProperty.Create(nameof(ReadOnly), typeof(bool), typeof(CheckBox), false, BindingMode.OneWay, null, ReadOnlyChanged);

        public bool Checked
        {
            get => (bool)GetValue(CheckedProperty);
            set => SetValue(CheckedProperty, value);
        }

        public bool ReadOnly
        {
            get => (bool)GetValue(ReadOnlyProperty);
            set => SetValue(ReadOnlyProperty, value);
        }

        private static void CheckedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CheckBox checkBox && newValue is bool isChecked)
            {
                checkBox.Checked = isChecked;
                checkBox.UpdateView();
            }
        }

        private static void ReadOnlyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CheckBox checkBox &&
                checkBox.TapRecognizer?.Command is Command command)
            {
                command.ChangeCanExecute();
            }
        }

        private void UpdateView()
        {
            UncheckedShape.IsVisible = !Checked;
            CheckedShape.IsVisible = Checked;
        }

        private bool CanChangeCheckbox() => !ReadOnly;
    }
}