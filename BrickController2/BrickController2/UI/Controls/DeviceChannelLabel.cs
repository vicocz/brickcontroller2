using System;
using Microsoft.Maui.Controls;
using BrickController2.DeviceManagement;
using BrickController2.Helpers;

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
        private readonly static string[] _mk6ChannelLetters = new[] { "A", "B", "C", "D", "E", "F" };

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

        private void SetChannelText()
        {
            switch (DeviceType)
            {
                case DeviceType.Boost:
                case DeviceType.DuploTrainHub:
                case DeviceType.PoweredUp:
                case DeviceType.TechnicHub:
                case DeviceType.WeDo2:
                    SetChannelText(_controlPlusChannelLetters);
                    break;
                case DeviceType.TechnicMove:
                    if (Channel == TechnicMoveDevice.CHANNEL_VM)
                        Text = "AB";
                    else
                        SetChannelText(_technicMove);
                    break;

                case DeviceType.PfxBrick:
                    SetChannelText(_pfxBricks);
                    break;

                case DeviceType.CircuitCubes:
                    SetChannelText(_circuitCubesChannelLetters);
                    break;

                case DeviceType.BuWizz3:
                    SetChannelText(_buwizz3ChannelLetters);
                    break;

                case DeviceType.Infrared:
                    Text = Channel == 0 ?
                        TranslationHelper.Translate("Blue") :
                        TranslationHelper.Translate("Red");
                    break;

                case DeviceType.MK4:
                case DeviceType.MK6:
                case DeviceType.MK_DIY:
                    SetChannelText(_mk6ChannelLetters);
                    break;

                case DeviceType.MK5:
                    SetChannelText(_mk5ChannelLetters);
                    break;

                default:
                    Text = $"{Channel + 1}";
                    break;
            }
        }

        private void SetChannelText(string[] labels)
            => Text = labels[Math.Min(Math.Max(Channel, 0), labels.Length - 1)];
    }
}
