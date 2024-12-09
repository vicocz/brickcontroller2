using System.Windows.Input;

namespace BrickController2.UI.Commands;

internal static class CommandExtensions
{
    internal static void RaiseCanExecuteChanged(this ICommand command)
    {
        if (command is IExtendableCommand cmd)
        {
            cmd.RaiseCanExecuteChanged();
        }
    }
}
