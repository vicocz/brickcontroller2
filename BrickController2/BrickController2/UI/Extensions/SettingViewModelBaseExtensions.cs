using BrickController2.UI.ViewModels.Settings;
using System.Collections.Generic;
using System.Linq;

namespace BrickController2.UI.Extensions;

internal static class SettingViewModelBaseExtensions
{
    public static void ResetToDefaults(this ICollection<SettingViewModelBase> viewModels)
    {
        foreach (var setting in viewModels.Where(s => s.HasNonDefaultValue))
        {
            setting.ResetToDefault();
        }
    }
}
