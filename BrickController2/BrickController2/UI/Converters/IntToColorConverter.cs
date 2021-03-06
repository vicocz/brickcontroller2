﻿using System;
using System.Globalization;
using Xamarin.Forms;

namespace BrickController2.UI.Converters
{
    public class IntToColorConverter : IValueConverter
    {
        private static readonly Color[] Colors = { Color.Brown, Color.DarkGreen, Color.DarkSlateGray, Color.DarkOrchid, Color.DimGray, Color.OliveDrab };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = (int)value;
            return Colors[intValue % Colors.Length];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
