using BrickController2.UI.Controls;
using Microsoft.Maui.Handlers;
using UIKit;

namespace BrickController2.iOS.UI.CustomRenderers
{
    public class ExtendedSliderRenderer : SliderHandler
    {
        protected override void ConnectHandler(UISlider platformView)
        {
            base.ConnectHandler(platformView);

            if (VirtualView is ExtendedSlider extendedSlider && platformView != null)
            {
                platformView.TouchDown += (sender, args) => extendedSlider.TouchDown();
                platformView.TouchUpInside += (sender, args) => extendedSlider.TouchUp();
                platformView.TouchUpOutside += (sender, args) => extendedSlider.TouchUp();
            }
        }
    }
}