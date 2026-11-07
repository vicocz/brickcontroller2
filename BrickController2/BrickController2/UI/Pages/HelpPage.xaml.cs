using BrickController2.UI.Services.Background;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace BrickController2.UI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HelpPage
    {
        public HelpPage(PageViewModelBase vm, IBackgroundService backgroundService, IDialogServerHost dialogServerHost)
            : base(backgroundService, dialogServerHost)
        {
            InitializeComponent();
            AfterInitialize(vm);
        }

        private void OnNavigating(object? sender, WebNavigatingEventArgs e)
        {
            if (BindingContext is HelpPageViewModel vm && vm.TryHandleNavigation(e.Url))
            {
                e.Cancel = true;
            }
        }
    }
}
