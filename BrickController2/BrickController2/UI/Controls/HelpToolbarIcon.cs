using BrickController2.Helpers;
using Microsoft.Maui.Controls;

namespace BrickController2.UI.Controls;

/// <summary>
/// Standard help toolbar item. Bind <see cref="MenuItem.Command"/> to the page help command.
/// </summary>
public class HelpToolbarIcon : ToolbarIcon
{
    public HelpToolbarIcon()
    {
        Icon = "help_outline";
        Order = ToolbarItemOrder.Secondary;
        Text = TranslationHelper.Translate("Help");
        SetBinding(CommandProperty, new Binding("ShowHelpCommand"));
    }
}
