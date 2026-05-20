using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using BrickController2.PlatformServices.InputDevice;

namespace BrickController2.UI.Converters
{
    public class GameControllerEventTypeToImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var eventType = (InputDeviceEventType)value!;
            return Convert(eventType);
        }

        public string? Convert(InputDeviceEventType eventType)
        {
            return eventType switch
            {
                InputDeviceEventType.Button => "abc",
                InputDeviceEventType.Axis => "gamepad",
                _ => null,
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
