using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using BrickController2.DeviceManagement;
using BrickController2.Helpers;

namespace BrickController2.UI.Converters
{
    public class DeviceTypeToImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var deviceType = (DeviceType)value!;
            return Convert(deviceType);
        }

        public ImageSource? Convert(DeviceType deviceType)
        {
            switch (deviceType)
            {
                case DeviceType.BuWizz:
                case DeviceType.BuWizz2:
                    return ResourceHelper.GetImageResource("buwizz_image.png");

                case DeviceType.BuWizz3:
                    return ResourceHelper.GetImageResource("buwizz3_image.png");

                case DeviceType.SBrick:
                    return ResourceHelper.GetImageResource("sbrick_image.png");

                case DeviceType.Infrared:
                    return ResourceHelper.GetImageResource("infra_image.png");

                case DeviceType.PoweredUp:
                    return ResourceHelper.GetImageResource("poweredup_image.png");

                case DeviceType.Boost:
                    return ResourceHelper.GetImageResource("boost_image.png");

                case DeviceType.TechnicHub:
                    return ResourceHelper.GetImageResource("technichub_image.png");

                case DeviceType.DuploTrainHub:
                    return ResourceHelper.GetImageResource("duplotrainhub_image.png");

                case DeviceType.CircuitCubes:
                    return ResourceHelper.GetImageResource("circuitcubes_image.png");

                case DeviceType.WeDo2:
                    return ResourceHelper.GetImageResource("wedo2hub_image.png");

                case DeviceType.TechnicMove:
                    return ResourceHelper.GetImageResource("technic_move.png");

                case DeviceType.PfxBrick:
                    return ResourceHelper.GetImageResource("pfx_brick_image.png");

                case DeviceType.MK3_8:
                    return ResourceHelper.GetImageResource("mk3_8_image.png");

                case DeviceType.MK4:
                    return ResourceHelper.GetImageResource("mk4_image.png");

                case DeviceType.MK5:
                    return ResourceHelper.GetImageResource("mk5_image.png");

                case DeviceType.MK6:
                    return ResourceHelper.GetImageResource("mk6_image.png");

                case DeviceType.MK_DIY:
                    return ResourceHelper.GetImageResource("mk_diy_image.png");

                case DeviceType.CaDA_RaceCar:
                    return ResourceHelper.GetImageResource("cada_racecar_image.png");

                case DeviceType.RemoteControl:
                    return ResourceHelper.GetImageResource("remotecontrol_image_small.png");

                default:
                    return null;
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
