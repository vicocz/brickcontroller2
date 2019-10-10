using BrickController2.UI.Controls;
using BrickController2.Windows.UI.CustomRenderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(ExtendedSlider), typeof(ExtendedSliderRenderer))]
namespace BrickController2.Windows.UI.CustomRenderers
{
    public class ExtendedSliderRenderer : SliderRenderer
    {
        public ExtendedSliderRenderer() : base()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
        {
            base.OnElementChanged(e);

            if (Element is ExtendedSlider extendedSlider && Control != null)
            {
                Control.PointerCaptureLost += (sender, args) => extendedSlider.TouchUp();

                if (extendedSlider.Step > 0)
                {
                    Control.StepFrequency = extendedSlider.Step;
                    Control.SmallChange = extendedSlider.Step;
                }
            }
        }
    }
}