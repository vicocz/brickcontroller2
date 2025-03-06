using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using BrickController2.DeviceManagement;

namespace BrickController2.UI.Converters
{
    public class DeviceTypeToGroupNameConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var deviceType = (DeviceType)value!;
            switch (deviceType)
            {
                case DeviceType.MK4:
                    return "Mould King - MK 4.0";

                case DeviceType.MK6:
                    return "Mould King - MK 6.0";

                default:
                    return $"{deviceType}";
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
