using System;
using Microsoft.Maui.Controls;
using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using BrickController2.DeviceManagement.Vengit;

namespace BrickController2.UI.Controls
{
    public class DeviceChannelLabel : Label
    {
        private readonly static string[] _controlPlusChannelLetters = new[] { "A", "B", "C", "D" };
        private readonly static string[] _technicMove = ["A", "B", "C", "1", "2", "3", "4", "5", "6"];
        private readonly static string[] _pfxBricks = ["A", "B", "1", "2", "3", "4", "5", "6", "7", "8"];
        private readonly static string[] _circuitCubesChannelLetters = new[] { "A", "B", "C" };
        private readonly static string[] _buwizz3ChannelLetters = new[] { "1", "2", "3", "4", "A", "B" };
        private readonly static string[] _mk5ChannelLetters = ["AB", "T", "C", "AB+T", "TL"];
        private readonly static string[] _mkDefaultChannelLetters = ["A", "B", "C", "D", "E", "F"];
        private readonly static string[] _sBrickLightChannelLetters = ["A", "B", "C", "D", "E", "F", "G", "H"];
        private readonly static string[] _jieStarChannelLetters = ["A", "B", "C", "D", "E", "F", "G", "H"];
        private readonly static string[] _powerBoxChannelLetters = ["A", "B", "C", "D"];

        public static readonly BindableProperty DeviceTypeProperty = BindableProperty.Create(nameof(DeviceType), typeof(DeviceType), typeof(DeviceChannelLabel), default(DeviceType), BindingMode.OneWay, null, OnDeviceChanged);
        public static readonly BindableProperty ChannelProperty = BindableProperty.Create(nameof(Channel), typeof(int), typeof(DeviceChannelLabel), 0, BindingMode.OneWay, null, OnChannelChanged);

        public DeviceType DeviceType
        {
            get => (DeviceType)GetValue(DeviceTypeProperty);
            set => SetValue(DeviceTypeProperty, value);
        }

        public int Channel
        {
            get => (int)GetValue(ChannelProperty);
            set => SetValue(ChannelProperty, value);
        }

        private static void OnDeviceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DeviceChannelLabel dcl)
            {
                dcl.SetChannelText();
            }
        }

        private static void OnChannelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DeviceChannelLabel dcl)
            {
                dcl.SetChannelText();
            }
        }

        private void SetChannelText() => Text = GetChannelText(DeviceType, Channel);

        /// <summary>
        /// Returns the channel label as displayed to the user for the given device type.
        /// </summary>
        public static string GetChannelText(DeviceType deviceType, int channel)
        {
            string Pick(string[] labels) => labels[Math.Min(Math.Max(channel, 0), labels.Length - 1)];

            switch (deviceType)
            {
                case DeviceType.Boost:
                case DeviceType.DuploTrainHub:
                case DeviceType.PoweredUp:
                case DeviceType.TechnicHub:
                case DeviceType.WeDo2:
                    return Pick(_controlPlusChannelLetters);
                case DeviceType.TechnicMove:
                    return channel == TechnicMoveDevice.CHANNEL_VM ? "AB" : Pick(_technicMove);

                case DeviceType.PfxBrick:
                    return Pick(_pfxBricks);

                case DeviceType.CircuitCubes:
                    return Pick(_circuitCubesChannelLetters);

                case DeviceType.BuWizz3:
                    return Pick(_buwizz3ChannelLetters);

                case DeviceType.Infrared:
                    return channel == 0 ?
                        TranslationHelper.Translate("Blue") :
                        TranslationHelper.Translate("Red");

                case DeviceType.MK3:
                case DeviceType.MK3_8:
                case DeviceType.MK4:
                case DeviceType.MK6:
                case DeviceType.MK_DIY:
                    return Pick(_mkDefaultChannelLetters);

                case DeviceType.MK5:
                    return Pick(_mk5ChannelLetters);

                case DeviceType.SBrickLight: // e.g. C or C.3
                    return channel < SBrickProtocol.LIGHT_PORTS_COUNT
                        ? Pick(_sBrickLightChannelLetters) // channel as base port
                        : $"{_sBrickLightChannelLetters[channel % 8]}.{channel / 8}"; // port with subchannel
                case DeviceType.JieStarSCM4:
                case DeviceType.JieStarSCM8:
                    return Pick(_jieStarChannelLetters);
                case DeviceType.PowerBoxMBattery:
                case DeviceType.PowerBoxASeries:
                    return Pick(_powerBoxChannelLetters);
                default:
                    return $"{channel + 1}";
            }
        }
    }
}
