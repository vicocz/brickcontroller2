using BrickController2.Windows.UI.CustomRenderers;
using System.Linq;
using Windows.UI.Xaml.Input;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(SwipeView), typeof(ExtendedSwipeViewRenderer))]
namespace BrickController2.Windows.UI.CustomRenderers
{
    public class ExtendedSwipeViewRenderer : SwipeViewRenderer
    {
        public ExtendedSwipeViewRenderer() : base()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<SwipeView> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.RightTapped += Control_RightTapped;
            }
        }

        private void Control_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // invoke command of the first left item to suppport deletion (workaround for Windouws without touch controls)
            var item = Element.LeftItems?.First();
            item?.Command?.Execute(item?.CommandParameter);
        }
    }
}