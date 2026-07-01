using System.Collections.Generic;
using System.Linq;
using Settings;
using UnityEngine;
using Utils.SO.Settings.Utils.SO.Settings;

namespace Managers
{
    public class SettingsManager : ManagerBase<SettingsManager>
    {
        [Header("Enforcers")]
        private List<ISettingEnforcer> _enforcers;
        private Dictionary<string, ISettingEnforcer> _enforcerLookup;
        
        [Header("Cached Settings")]
        private readonly Dictionary<string, int> cachedIntChanges = new();
        private readonly Dictionary<string, float> cachedFloatChanges = new();
        private readonly Dictionary<string, string> cachedStringChanges = new();
#if UNITY_EDITOR
        public IReadOnlyDictionary<string, int> DebugInts => cachedIntChanges;
        public IReadOnlyDictionary<string, float> DebugFloats => cachedFloatChanges;
        public IReadOnlyDictionary<string, string> DebugStrings => cachedStringChanges;
#endif
        
        [Header("Input")]
        [SerializeField] private InputAssignments defaultInputSettings;
        private Dictionary<string, string> _defaultInputDict;

        protected override void Awake()
        {
            base.Awake();

            _defaultInputDict = defaultInputSettings.AsDictionary();

            SettingEnforcerRegistry.Initialize();

            _enforcers =
                SettingEnforcerRegistry.Enforcers
                    .ToList();

            _enforcerLookup =
                _enforcers.ToDictionary(
                    x => x.GetKey(),
                    x => x);
        }
        
        private ISettingEnforcer FindEnforcer(string fullName)
        {
            if (_enforcerLookup.TryGetValue(
                    fullName,
                    out var exact))
            {
                return exact;
            }


            var parts =
                fullName.Split('/');


            if (parts.Length < 3) return parts.Length < 1 ? null : _enforcerLookup.GetValueOrDefault(parts[0]);
            var category =
                $"{parts[0]}/{parts[1]}";


            if (_enforcerLookup.TryGetValue(
                    category,
                    out var categoryEnforcer))
            {
                return categoryEnforcer;
            }


            return parts.Length < 1 ? null : _enforcerLookup.GetValueOrDefault(parts[0]);
        }

        public static int GetIntSetting(string key, int defaultValue) => Instance.GetInt(key, defaultValue);
        private int GetInt(string key, int defaultValue)
        {
            if (cachedIntChanges.TryGetValue(key, out var v)) return v;
            var value = PlayerPrefs.GetInt(key, defaultValue);
            cachedIntChanges.Add(key, value);
            PlayerPrefs.SetInt(key, value);
            return value;
        }
        
        public static float GetFloatSetting(string key, float defaultValue) => Instance.GetFloat(key, defaultValue);
        private float GetFloat(string key, float defaultValue)
        {
            if (cachedFloatChanges.TryGetValue(key, out var v)) return v;
            var value = PlayerPrefs.GetFloat(key, defaultValue);
            cachedFloatChanges.Add(key, value);
            PlayerPrefs.SetFloat(key, value);
            return value;
        }
        
        public static string GetStringSetting(string key, string defaultValue) => Instance.GetString(key, defaultValue);
        private string GetString(string key, string defaultValue)
        {
            if (cachedStringChanges.TryGetValue(key, out var v))
                return v;

            var value = PlayerPrefs.GetString(key, defaultValue);

            if (string.IsNullOrWhiteSpace(value))
                value = defaultValue;

            cachedStringChanges.Add(key, value);
            PlayerPrefs.SetString(key, value);

            return value;
        }
        
        public static string GetDefaultInputSetting(string key) => Instance.GetDefaultInput(key);

        private string GetDefaultInput(string key)
        {
            _defaultInputDict.TryGetValue(key, out var value);
            return value;
        }
        
        public static void SaveIntSetting(string key, int value)
        {
            Instance.SaveInt(key, value);
        }

        private void SaveInt(string key, int value)
        {
            cachedIntChanges[key] = value;

            PlayerPrefs.SetInt(
                key,
                value);


            FindEnforcer(key)?
                .Enforce(
                    key,
                    value);
        }


        public static void SaveFloatSetting(string key, float value)
        {
            Instance.SaveFloat(key, value);
        }

        private void SaveFloat(string key, float value)
        {
            cachedFloatChanges[key] = value;

            PlayerPrefs.SetFloat(
                key,
                value);


            FindEnforcer(key)?
                .Enforce(
                    key,
                    value);
        }

        public static void SaveStringSetting(string key, string value)
        {
            Instance.SaveString(key, value);
        }

        private void SaveString(string key, string value)
        {
            cachedStringChanges[key] = value;

            PlayerPrefs.SetString(
                key,
                value);


            FindEnforcer(key)?
                .Enforce(
                    key,
                    value);
        }

        public static void SaveSettings(
            Dictionary<string, int> intSettings,
            Dictionary<string, float> floatSettings,
            Dictionary<string, string> stringSettings)
        {
            Instance.Save(intSettings, floatSettings, stringSettings);
        }

        private void Save(
            Dictionary<string, int> intSettings,
            Dictionary<string, float> floatSettings,
            Dictionary<string, string> stringSettings)
        {
            foreach (var pair in intSettings)
            {
                cachedIntChanges[pair.Key] = pair.Value;

                PlayerPrefs.SetInt(
                    pair.Key,
                    pair.Value);


                FindEnforcer(pair.Key)?
                    .Enforce(
                        pair.Key,
                        pair.Value);
            }

            foreach (var pair in floatSettings)
            {
                cachedFloatChanges[pair.Key] = pair.Value;

                PlayerPrefs.SetFloat(
                    pair.Key,
                    pair.Value);


                FindEnforcer(pair.Key)?
                    .Enforce(
                        pair.Key,
                        pair.Value);
            }

            foreach (var pair in stringSettings)
            {
                cachedStringChanges[pair.Key] = pair.Value;

                PlayerPrefs.SetString(
                    pair.Key,
                    pair.Value);


                FindEnforcer(pair.Key)?
                    .Enforce(
                        pair.Key,
                        pair.Value);
            }

            PlayerPrefs.Save();
        }
    }
}