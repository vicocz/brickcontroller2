using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Autofac;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Controls.Devices;
using BrickController2.UI.Pages;
using BrickController2.UI.Services.Background;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Help;
using BrickController2.UI.Services.Localization;
using BrickController2.UI.Services.MainThread;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Preferences;
using BrickController2.UI.Services.Settings;
using BrickController2.UI.Services.Theme;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.ViewModels;
using BrickController2.UI.Services.AppIdentifier;

namespace BrickController2.UI.DI
{
    public class UiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register services

            builder.RegisterType<NavigationService>().As<INavigationService>().SingleInstance();
            builder.RegisterType<MainThreadService>().As<IMainThreadService>().SingleInstance();
            builder.RegisterType<BackgroundService>().AsSelf().As<IBackgroundService>().SingleInstance();
            builder.RegisterType<TranslationService>().AsSelf().As<ITranslationService>().SingleInstance();
            builder.RegisterType<PreferencesService>().AsSelf().As<IPreferencesService>().SingleInstance();
            builder.RegisterType<ThemeService>().AsSelf().As<IThemeService>().SingleInstance();
            builder.RegisterType<LocalizationService>().AsSelf().As<ILocalizationService>().SingleInstance();
            builder.RegisterType<AppIdentifierService>().AsSelf().As<IAppIdentifierService>().SingleInstance();
            builder.RegisterType<HelpService>().As<IHelpService>().SingleInstance();

            // Register Dialogs
            builder.RegisterType<DialogService>().As<IDialogService>().As<IDialogServerHost>().SingleInstance();

            // Register viewmodels, but exclude abstract ones
            foreach (var vmType in GetSubClassesOf<PageViewModelBase>().Where(t => !t.IsAbstract))
            {
                builder.RegisterType(vmType).Keyed<PageViewModelBase>(vmType);
            }

            // Register pages
            foreach (var pageType in GetSubClassesOf<PageBase>())
            {
                builder.RegisterType(pageType).Keyed<PageBase>(pageType);
            }

            // Register device channel selector views
            foreach (var viewType in GetSubClassesOf<DeviceChannelSelectorViewBase>().Where(t => !t.IsAbstract && typeof(IDeviceChannelSelectorView).IsAssignableFrom(t)))
            {
                var deviceType = GetDeviceType(viewType);
                builder.RegisterType(viewType).Keyed<DeviceChannelSelectorViewBase>(deviceType);
            }

            // custom channel selectors
            builder.RegisterType<BuWizzChannelSelectorView>().Keyed<DeviceChannelSelectorViewBase>(DeviceType.BuWizz2);

            // Built-in rows of the settings page (instance per dependency = one set per settings page).
            // Platform projects register their own rows the same way; the view model sorts by ISettingsItem.Order,
            // so the result does not depend on the module registration order (platform modules are registered before this one).
            builder.RegisterType<ThemeSettingsItem>().As<ISettingsItem>().InstancePerDependency();
            builder.Register(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return new LanguageSettingsItem(
                        context.Resolve<ILocalizationService>(),
                        context.Resolve<IDialogService>(),
                        context.Resolve<INavigationService>(),
                        context.Resolve<ITranslationService>(),
                        () => (Application.Current as App)?.ReloadRootPage());
                })
                .As<ISettingsItem>()
                .InstancePerDependency();
            builder.RegisterType<OrientationSensorSettingsItem>().As<ISettingsItem>().InstancePerDependency();

            // Register the viewmodel factory
            builder.Register<ViewModelFactory>(c =>
            {
                var componentContext = c.Resolve<IComponentContext>();
                return (type, parameters) => componentContext.ResolveKeyed<PageViewModelBase>(type, new TypedParameter(typeof(NavigationParameters), parameters));
            });

            // Register the page factory
            builder.Register<PageFactory>(c =>
            {
                var componentContext = c.Resolve<IComponentContext>();
                return (type, vm) => componentContext.ResolveKeyed<PageBase>(type, new TypedParameter(typeof(PageViewModelBase), vm));
            });

            // command related registration
            builder.RegisterType<CreationCommandFactory>().As<ICommandFactory<Creation>>().SingleInstance();
            builder.RegisterType<SequenceCommandFactory>().As<ICommandFactory<Sequence>>().SingleInstance();

            // Xamarin forms related
            builder.RegisterType<NavigationPage>();
            builder.RegisterType<App>();
        }

        private IEnumerable<Type> GetSubClassesOf<T>()
        {
            return ThisAssembly.GetTypes()
                .Where(t => t != typeof(T) && typeof(T).IsAssignableFrom(t))
                .ToList();
        }

        private static DeviceType GetDeviceType(Type viewType)
        {
            var method = typeof(UiModule)
                .GetMethod(nameof(GetDeviceTypeCore), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(viewType);

            return (DeviceType)method.Invoke(null, null)!;
        }

        private static DeviceType GetDeviceTypeCore<T>() where T : IDeviceChannelSelectorView
            => T.DeviceType;
    }
}
