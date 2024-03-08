using System.Windows.Input;

namespace BrickController2.UI.Commands;

internal interface IExtendableCommand
{
    void RaiseCanExecuteChanged();
}
