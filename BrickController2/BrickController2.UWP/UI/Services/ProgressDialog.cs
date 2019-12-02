using BrickController2.UI.Services.Dialog;
using Xamarin.Forms.Platform.UWP;
using ProgressBar = Windows.UI.Xaml.Controls.ProgressBar;

namespace BrickController2.Windows.UI.Services
{
    public class ProgressDialog : IProgressDialog
    {
        private readonly ProgressBar _progressBar;
        private readonly AlertDialog _progressDialog;
        
        public ProgressDialog(AlertDialog progressDialog, ProgressBar progressBar)
        {
            _progressDialog = progressDialog;
            _progressBar = progressBar;
        }

        public string Title
        {
            set => _progressDialog.Title = value;
        }

        public string Message
        {
            set => _progressDialog.Content = value;
        }

        public int Percent
        {
            get => (int)_progressBar.Value;
            set => _progressBar.Value = value;
        }
    }
}