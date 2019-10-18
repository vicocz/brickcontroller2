using BrickController2.UI.Services.Background;
using BrickController2.UI.ViewModels;
using Xamarin.Forms.Xaml;

namespace BrickController2.UI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BlocksTesterPage 
    {
        public BlocksTesterPage(PageViewModelBase vm, IBackgroundService backgroundService) : base(backgroundService)
        {
            InitializeComponent();
            BindingContext = vm;

            this.webView.Navigating += WebView_Navigating;
            this.webView.Navigated += WebView_Navigated;
        }

        private void WebView_Navigated(object sender, Xamarin.Forms.WebNavigatedEventArgs e)
        {

        }

        private void WebView_Navigating(object sender, Xamarin.Forms.WebNavigatingEventArgs e)
        {

        }
    }
}