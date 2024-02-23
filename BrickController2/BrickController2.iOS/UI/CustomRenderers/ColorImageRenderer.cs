using BrickController2.UI.Controls;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;
using UIKit;

namespace BrickController2.iOS.UI.CustomRenderers
{
    public class ColorImageRenderer : ImageHandler
    {
        public static readonly PropertyMapper<ColorImage, ColorImageRenderer> PropertyMapper = new(ImageHandler.Mapper)
        {
            [ColorImage.ColorProperty.PropertyName] = SetColor,
            [ColorImage.SourceProperty.PropertyName] = SetColor,
            [ColorImage.IsLoadingProperty.PropertyName] = SetColor,
        };

        public ColorImageRenderer() : base(PropertyMapper)
        {
        }

        protected override void ConnectHandler(UIImageView platformView)
        {
            base.ConnectHandler(platformView);
            SetColor(this, VirtualView as ColorImage);
        }

        private static void SetColor(ColorImageRenderer renderer, ColorImage colorImage)
        {
            if (renderer?.PlatformView?.Image == null || colorImage is null)
            {
                return;
            }

            if (colorImage.Color.Equals(Colors.Transparent))
            {
                renderer.PlatformView.Image = renderer.PlatformView.Image.ImageWithRenderingMode(UIImageRenderingMode.Automatic);
                renderer.PlatformView.TintColor = null;
            }
            else
            {
                renderer.PlatformView.Image = renderer.PlatformView.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                renderer.PlatformView.TintColor = colorImage.Color.AsUIColor();
            }
        }
    }
}