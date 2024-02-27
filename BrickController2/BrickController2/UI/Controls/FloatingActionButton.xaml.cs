﻿using System.Windows.Input;

namespace BrickController2.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FloatingActionButton : ContentView
    {
        public FloatingActionButton()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty ButtonColorProperty = BindableProperty.Create(nameof(ButtonColor), typeof(Color), typeof(FloatingActionButton), default(Color), BindingMode.OneWay, null, ButtonColorChanged);
        public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(FloatingActionButton), null, BindingMode.OneWay, null, IconChanged);
        public static readonly BindableProperty ImageColorProperty = BindableProperty.Create(nameof(ImageColor), typeof(Color), typeof(FloatingActionButton), null, BindingMode.OneWay, null, ImageColorChanged);
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(FloatingActionButton), null, BindingMode.OneWay, null, CommandChanged);

        public Color ButtonColor
        {
            get => (Color)GetValue(ButtonColorProperty);
            set => SetValue(ButtonColorProperty, value);
        }

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

        private static void ButtonColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is FloatingActionButton fab && newValue is Color backgroundColor)
            {
                fab.ImageFrame.BackgroundColor = backgroundColor;
            }
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

        private static void CommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is FloatingActionButton fab && newValue is ICommand command)
            {
                fab.TapGuestureRecognizer.Command = command;
            }
        }
    }
}