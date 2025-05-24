using System;
using System.Globalization;
using BrickController2.CreationManagement;
using Microsoft.Maui.Controls;

namespace BrickController2.UI.Converters;

internal class ServoOutputTypeToVisibleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
         => value is ChannelOutputType outputType && outputType == ChannelOutputType.ServoMotor;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
         => throw new NotImplementedException();
}