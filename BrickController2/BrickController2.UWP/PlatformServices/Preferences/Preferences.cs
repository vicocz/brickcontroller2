using System;
using BrickController2.PlatformServices.Preferences;
using Windows.Storage;

namespace BrickController2.Windows.PlatformServices.Preferences
{
    public class Preferences : IPreferences
    {
        private readonly object _lock = new object();

        public Preferences()
        {
        }

        public bool ContainsKey(string key, string section = null)
        {
            lock (_lock)
            {
                var container = GetApplicationDataContainer(section);
                return container.Values.ContainsKey(key);
            }
        }

        public T Get<T>(string key, string section = null)
        {
            lock (_lock)
            {
                var container = GetApplicationDataContainer(section);

                if (!container.Values.ContainsKey(key))
                {
                    throw new ArgumentException(nameof(key));
                }

                return Get<T>(key, container);
            }
        }

        public T Get<T>(string key, T defaultValue, string section = null)
        {
            lock (_lock)
            {
                var container = GetApplicationDataContainer(section);
                if (!container.Values.ContainsKey(key))
                {
                    return defaultValue;
                }

                return Get<T>(key, container);
            }
        }

        public void Set<T>(string key, T value, string section = null)
        {
            lock (_lock)
            {
                var container = GetApplicationDataContainer(section);

                switch (value)
                {
                    case bool b:
                        container.Values[key] = b;
                        break;

                    case int i:
                        container.Values[key] = i;
                        break;

                    case float f:
                        container.Values[key] = f;
                        break;

                    case string s:
                        container.Values[key] = s;
                        break;

                    default:
                        throw new NotSupportedException($"{typeof(T)} is not supported.");
                }
            }
        }

        private ApplicationDataContainer GetApplicationDataContainer(string section)
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            if (!string.IsNullOrEmpty(section))
            {
                return localSettings.CreateContainer(section, ApplicationDataCreateDisposition.Always);
            }
            return localSettings;
        }

        private T Get<T>(string key, ApplicationDataContainer container)
        {
            object result = null;

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    result = (bool)container.Values[key];
                    break;

                case TypeCode.Int32:
                    result = (int)container.Values[key];
                    break;

                case TypeCode.Single:
                    result = (float)container.Values[key];
                    break;

                case TypeCode.String:
                    result = (string)container.Values[key];
                    break;

                default:
                    throw new NotSupportedException($"{typeof(T)} is not supported.");
            }

            return (T)result;
        }
    }
}