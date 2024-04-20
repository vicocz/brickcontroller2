using BrickController2.PlatformServices.Localization;
using System.Globalization;
using Tizen.System;

[assembly:Dependency(typeof(BrickController2.Tizen.PlatformServices.Localization.LocalizationService))]
namespace BrickController2.Tizen.PlatformServices.Localization
{
    public class LocalizationService : ILocalizationService
    {
        private string _tizenLocale;
        private CultureInfo _ci = null;

        public CultureInfo CurrentCultureInfo
        {
            get
            {
                if (_ci == null || _tizenLocale != SystemSettings.LocaleLanguage)
                {
                    _tizenLocale = SystemSettings.LocaleLanguage;
                    var netLanguage = TizenToDotnetLanguage(_tizenLocale.Replace("_", "-"));

                    try
                    {
                        _ci = new CultureInfo(netLanguage);
                    }
                    catch (CultureNotFoundException)
                    {
                        try
                        {
                            var fallback = ToDotnetFallbackLanguage(new PlatformCulture(netLanguage));
                            _ci = new CultureInfo(fallback);
                        }
                        catch (CultureNotFoundException)
                        {
                            _ci = new CultureInfo("en");
                        }
                    }
                }

                return _ci;
            }

            set
            {
                Thread.CurrentThread.CurrentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;
            }
        }

        static string TizenToDotnetLanguage(string tizenLanguage)
        {
            //certain languages need to be converted to CultureInfo equivalent
            return tizenLanguage switch
            {
                // Chinese Simplified (People's Republic of China)
                "zh-CN" => "zh-Hans",// correct code for .NET
                                     // Chinese Traditional (Hong Kong)
                "zh-HK" or "zh-hk" or "zh-tw" or "zh-TW" => "zh-Hant",// correct code for .NET
                _ => tizenLanguage,
            };
        }

        private static string ToDotnetFallbackLanguage(PlatformCulture platCulture)
        {
            var netLanguage = platCulture.LanguageCode; // use the first part of the identifier (two chars, usually);
            switch (platCulture.LanguageCode)
            {
                case "gsw":
                    netLanguage = "de-CH"; // equivalent to German (Switzerland) for this app
                    break;
                    // add more application-specific cases here (if required)
                    // ONLY use cultures that have been tested and known to work
            }

            return netLanguage;
        }
    }
}