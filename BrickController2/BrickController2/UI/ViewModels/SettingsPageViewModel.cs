using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Settings;
using BrickController2.UI.Services.Translation;
using System.Collections.Generic;
using System.Linq;

namespace BrickController2.UI.ViewModels
{
    /// <summary>
    /// View model of the settings page. It only renders the rows it receives as <see cref="ISettingsItem"/>
    /// (built-in rows and platform rows, all registered in Autofac) and attaches/detaches them with the page.
    /// </summary>
    public class SettingsPageViewModel : PageViewModelBase
    {
        public SettingsPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IEnumerable<ISettingsItem> settingsItems) :
            base(navigationService, translationService)
        {
            // OrderBy is stable: rows with equal Order keep their registration order
            SettingsItems = settingsItems.OrderBy(i => i.Order).ToList();
        }

        /// <summary>
        /// All rows of the settings page in display order (ascending <see cref="ISettingsItem.Order"/>).
        /// </summary>
        public IReadOnlyList<ISettingsItem> SettingsItems { get; }

        public override void OnAppearing()
        {
            base.OnAppearing();
            foreach (var item in SettingsItems)
            {
                item.Attach();
            }
        }

        public override void OnDisappearing()
        {
            foreach (var item in SettingsItems)
            {
                item.Detach();
            }
            base.OnDisappearing();
        }
    }
}
