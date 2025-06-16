using System;
using System.Globalization;
using BrickController2.CreationManagement;
using Microsoft.Maui.Controls;

namespace BrickController2.UI.Converters;

internal class StepperOutputTypeToVisibleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
         => value is ChannelOutputType outputType && outputType == ChannelOutputType.StepperMotor;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
         => throw new NotImplementedException();
}