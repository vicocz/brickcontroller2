using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BrickController2.UI.Services.Navigation
{
    public class NavigationParameters
    {
        // ignore casing
        private readonly IDictionary<string, object> _parameters = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

        public NavigationParameters(params (string Key, object Value)[] parameters)
        {
            foreach(var (Key, Value) in parameters)
            {
                _parameters[Key] = Value;
            }
        }

        public NavigationParameters(object value, [CallerArgumentExpression(nameof(value))] string key = "")
        {
            _parameters[key] = value;
        }

        public bool Contains(string key)
        {
            return _parameters.ContainsKey(key);
        }

        public T Get<T>(string key)
        {
            if (!_parameters.TryGetValue(key, out object? value))
            {
                throw new ArgumentException($"No parameter for key '{key}'.");
            }

            if (value is not T typedValue)
            {
                throw new ArgumentException($"Parameter for '{key}' is not type of '{typeof(T).Name}'.");
            }

            return typedValue;
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (!_parameters.TryGetValue(key, out object? value))
            {
                return defaultValue;
            }

            if (value is not T typedValue)
            {
                return defaultValue;
            }

            return typedValue;
        }
    }
}
