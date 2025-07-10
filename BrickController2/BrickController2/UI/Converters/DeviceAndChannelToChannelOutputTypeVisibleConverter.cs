using System;
using System.Globalization;
using BrickController2.CreationManagement;
using Microsoft.Maui.Controls;
using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.Converters
{
    public class DeviceAndChannelToChannelOutputTypeVisibleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is Device device && values[1] is int channel)
            {
                return device.IsOutputTypeSupported(channel, ChannelOutputType.ServoMotor) ||
                     device.IsOutputTypeSupported(channel, ChannelOutputType.StepperMotor);
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
