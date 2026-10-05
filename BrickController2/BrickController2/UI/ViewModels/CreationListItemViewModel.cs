using BrickController2.CreationManagement;
using System.ComponentModel;

namespace BrickController2.UI.ViewModels;

public sealed class CreationListItemViewModel(Creation creation, string assignmentText) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private bool _isSelected;
    public Creation Creation { get; } = creation;
    public string AssignmentText { get; } = assignmentText;
    public bool CanSelect => Creation.ControllerAssignmentId != null && Creation.ControllerAssignmentId != "none";
    public bool CannotSelect => !CanSelect;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            var selected = value && CanSelect;
            if (_isSelected == selected) return;
            _isSelected = selected;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }
}
