using BrickController2.UI.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Linq;

namespace BrickController2.Windows.UI.CustomHandlers;

public class CustomSwipeViewHandler : SwipeViewHandler
{
    protected override void ConnectHandler(SwipeControl platformView)
    {
        base.ConnectHandler(platformView);

        platformView.RightTapped += SwipeControl_RightTapped;
    }

    protected override void DisconnectHandler(SwipeControl platformView)
    {
        platformView.RightTapped -= SwipeControl_RightTapped;

        base.DisconnectHandler(platformView);
    }

    private void SwipeControl_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        // open context menu instead of swipte items
        if (VirtualView.LeftItems.Count == 0 && VirtualView.RightItems.Count == 0)
        {
            return;
        }

        var contextMenu = new MenuFlyout();

        foreach (var item in VirtualView.LeftItems
            .Concat(VirtualView.RightItems)
            .Cast<SwipeIcon>()
            .Where(x => x.IsEnabled && x.IsVisible))
        {
            contextMenu.Items.Add(new MenuFlyoutItem
            {
                Icon = GetIconElement(item),
                Text = item.Text,
                Command = item.Command,
                CommandParameter = item.CommandParameter,
            });
        }
        contextMenu.ShowAt(PlatformView, e.GetPosition(PlatformView));
    }

    private IconElement? GetIconElement(SwipeIcon item)
    {
        var iconSource = item.IconImageSource.ToIconSource(MauiContext!);
        if (iconSource is FontIconSource fontIconSource)
        {
            // hardcode now to override SwipeItem's Icon color which is typically white
            fontIconSource.Foreground = Colors.Black.ToPlatform();
        }
        return iconSource?.CreateIconElement();
    }
}