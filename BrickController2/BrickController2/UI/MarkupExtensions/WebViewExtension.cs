using System;
using BrickController2.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrickController2.UI.MarkupExtensions
{
    [ContentProperty(nameof(Source))]
    public class WebViewExtension : IMarkupExtension<string>
    {
        public string Source { get; set; }

        public string ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValueInternal();
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValueInternal();
        }

        private string ProvideValueInternal()
        {
            if (string.IsNullOrEmpty(Source))
            {
                return Source;
            }

            return $"ms-appdata://local/BLOCKLI/{Source}";
        }
    }
}
