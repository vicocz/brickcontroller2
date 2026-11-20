using BrickController2.Helpers;

namespace BrickController2.UI.Services.Navigation;

public static class RootKeys
{
    public const string Creations = nameof(Creations);
    public const string Devices = nameof(Devices);
    public const string Sequences = nameof(Sequences);
    public const string More = nameof(More);
}

/// <summary>
/// Shared state of the bottom navigation bar.
/// </summary>
public class NavigationState : NotifyPropertyChangedSource
{
    private string _activeRootKey = RootKeys.Creations;
    private bool _isMoreMenuOpen;

    public string ActiveRootKey
    {
        get => _activeRootKey;
        set
        {
            if (_activeRootKey != value)
            {
                _activeRootKey = value;
                RaisePropertyChanged();
            }
        }
    }

    public bool IsMoreMenuOpen
    {
        get => _isMoreMenuOpen;
        set
        {
            if (_isMoreMenuOpen != value)
            {
                _isMoreMenuOpen = value;
                RaisePropertyChanged();
            }
        }
    }
}
