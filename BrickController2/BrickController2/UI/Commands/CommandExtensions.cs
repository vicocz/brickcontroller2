using System.Windows.Input;

namespace BrickController2.UI.Commands;

public static class CommandExtensions
{
    public static void RaiseCanExecuteChanged(this ICommand command)
    {
        if (command is SafeCommand cmd)
        {
            cmd.RaiseCanExecuteChanged();
        }
        //else if (command is SafeCommand<> cmd)
        //{
        //    cmd.RaiseCanExecuteChanged();
        //}
    }
}
