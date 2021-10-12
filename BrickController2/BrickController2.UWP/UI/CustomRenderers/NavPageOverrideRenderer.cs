using BrickController2.Windows.UI.CustomRenderers;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(NavPageOverrideRenderer))]
namespace BrickController2.Windows.UI.CustomRenderers
{
    public class NavPageOverrideRenderer : NavigationPageRenderer
    {
        public NavPageOverrideRenderer()
        {
        }

        private void WorkaroundHideTitle(Page page)
        {
            page.Title = string.Empty;
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            if (e.NewElement is NavigationPage page)
            {
                WorkaroundHideTitle(page.CurrentPage);
            }

            base.OnElementChanged(e);
        }

        protected override void OnPopRequested(object sender, NavigationRequestedEventArgs e)
        {
            WorkaroundHideTitle(e.Page);

            base.OnPopRequested(sender, e);
        }

        protected override void OnPopToRootRequested(object sender, NavigationRequestedEventArgs e)
        {
            WorkaroundHideTitle(e.Page);

            base.OnPopToRootRequested(sender, e);
        }

        protected override void OnPushRequested(object sender, NavigationRequestedEventArgs e)
        {
            WorkaroundHideTitle(e.Page);

            base.OnPushRequested(sender, e);
        }
    }
}
