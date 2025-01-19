using BrickController2.UI.Controls;
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
                Icon = item.IconImageSource.ToIconSource(MauiContext!)?.CreateIconElement(),
                Text = item.Text,
                Command = item.Command,
                CommandParameter = item.CommandParameter,
            });
        }
        contextMenu.ShowAt(PlatformView, e.GetPosition(PlatformView));
    }

    protected override void DisconnectHandler(SwipeControl platformView)
    {
        platformView.RightTapped -= SwipeControl_RightTapped;

        base.DisconnectHandler(platformView);
    }
}