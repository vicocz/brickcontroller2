using BrickController2.DeviceManagement;
using BrickController2.Extensions;
using BrickController2.Helpers;
using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace BrickController2.UI.Converters
{
    public class DeviceTypeToGroupNameConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var deviceType = (DeviceType)value!;
            // compose group name based on localized string of vendor and device type
            return $"{TranslationHelper.Translate(deviceType.GetVendor())} - {TranslationHelper.Translate(deviceType)}";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
