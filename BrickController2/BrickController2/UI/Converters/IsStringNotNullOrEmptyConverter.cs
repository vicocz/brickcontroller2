using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace BrickController2.UI.Converters
{
    public class IsStringNotNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var stringValue = (string)value!;
            return !string.IsNullOrEmpty(stringValue);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
