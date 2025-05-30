using BrickController2.UI.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using UIKit;

namespace BrickController2.iOS.UI.CustomHandlers
{
    internal class ExtendedSliderHandler : SliderHandler
    {
        public static readonly PropertyMapper<ExtendedSlider, ExtendedSliderHandler> _PropertyMapper = new(SliderHandler.Mapper)
        {
            [nameof(ExtendedSlider.Step)] = MapValue
        };

        private ExtendedSlider? Slider => VirtualView as ExtendedSlider;

        public ExtendedSliderHandler() : base(_PropertyMapper)
        {
        }

        protected override void ConnectHandler(UISlider platformView)
        {
            base.ConnectHandler(platformView);

            platformView.TouchDown += StartTrackingTouch;
            platformView.TouchUpInside += StopTrackingTouch;
            platformView.TouchUpOutside += StopTrackingTouch;

            platformView.ValueChanged += OnControlValueChanged;
        }

        protected override void DisconnectHandler(UISlider platformView)
        {
            platformView.TouchDown -= StartTrackingTouch;
            platformView.TouchUpInside -= StopTrackingTouch;
            platformView.TouchUpOutside -= StopTrackingTouch;

            platformView.ValueChanged -= OnControlValueChanged;

            platformView.Dispose();
            base.DisconnectHandler(platformView);
        }

        private void StartTrackingTouch(object? sender, EventArgs e)
        {
            Slider?.TouchDown();
        }

        private void StopTrackingTouch(object? sender, EventArgs e)
        {
            Slider?.TouchUp();
        }

        void OnControlValueChanged(object? sender, EventArgs eventArgs)
        {
            if (sender is UISlider platformView)
            {
                Slider?.SetAndRoundNewValue(platformView.Value);
            }
        }
    }
}
