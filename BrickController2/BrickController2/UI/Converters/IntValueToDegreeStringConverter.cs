using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace BrickController2.UI.Converters;

public class IntValueToDegreeStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            // Convert integer value to a degree string representation
            return $"{intValue}°";
        }
        return value!;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new InvalidOperationException("ConvertBack is not supported for IntValueToDegreeStringConverter.");
}
