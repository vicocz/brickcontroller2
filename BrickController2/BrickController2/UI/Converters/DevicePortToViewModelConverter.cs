using BrickController2.DeviceManagement;
using BrickController2.UI.ViewModels;
using System;
using System.Globalization;


namespace BrickController2.UI.Converters
{
    public class DevicePortToViewModelConverter : Xamarin.Forms.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var device = parameter as Device;
            var port = value as DevicePort;

            return new ChannelOutputViewModel(device, port);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
