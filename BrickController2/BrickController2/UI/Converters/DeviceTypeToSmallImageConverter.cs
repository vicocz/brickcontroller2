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
            var deviceType = (DeviceType)value!;
            if (deviceType == DeviceType.Unknown)
                return null;

            var imageInfo = GetImage(deviceType);
            if (imageInfo is null)
                return null;

            return ResourceHelper.GetImageResource(imageInfo.SmallImageResourceName);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
