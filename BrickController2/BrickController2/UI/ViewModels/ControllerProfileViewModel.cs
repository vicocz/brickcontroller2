using BrickController2.CreationManagement;
using BrickController2.Helpers;

namespace BrickController2.UI.ViewModels;

public class ControllerProfileViewModel : NotifyPropertyChangedSource
{
    private readonly PlayerPageViewModel _parent;

    public ControllerProfileViewModel(PlayerPageViewModel parent, ControllerProfile profile)
    {
        _parent = parent;
        Profile = profile;
    }

    public ControllerProfile Profile { get; }
    public string Name => Profile.Name;

    public bool ShowCurrentProfile => IsActive && _parent.ControllerProfiles.Count > 1;

    public bool IsActive => _parent.ActiveProfileInternal == Profile;

    internal void NotifyPropertyChanges()
    {
        RaisePropertyChanged(nameof(IsActive));
        RaisePropertyChanged(nameof(ShowCurrentProfile));
    }
}
