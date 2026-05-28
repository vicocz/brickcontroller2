using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace BrickController2.UI.Converters
{
    public class DeviceTypeToSmallImageConverter : DeviceTypeToImageConverterBase, IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (TryGetImage((DeviceType)value!, out var imageInfo))
            {
                return ResourceHelper.GetImageResource(imageInfo.SmallImageResourceName);
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
