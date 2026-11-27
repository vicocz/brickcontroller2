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
/// Shared state of the tab navigation.
/// </summary>
public class NavigationState : NotifyPropertyChangedSource
{
    private string _activeRootKey = RootKeys.Creations;
    private bool _isNavigationEnabled = true;

    public bool IsNavigationEnabled
    {
        get => _isNavigationEnabled;
        set
        {
            if (_isNavigationEnabled != value)
            {
                _isNavigationEnabled = value;
                RaisePropertyChanged();
            }
        }
    }

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
}
