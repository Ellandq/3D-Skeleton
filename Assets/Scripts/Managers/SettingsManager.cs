using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class SettingsManager : ManagerBase<SettingsManager>
    {
        [Header("Cached Settings")]
        private Dictionary<string, int> cachedIntChanges = new();
        private Dictionary<string, float> cachedFloatChanges = new();
        private Dictionary<string, string> cachedStringChanges = new();

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
            if (cachedStringChanges.TryGetValue(key, out var v)) return v;
            var value = PlayerPrefs.GetString(key, defaultValue);
            cachedStringChanges.Add(key, value);
            PlayerPrefs.SetString(key, value);
            return value;
        }
        
        public static void SaveIntSetting(string key, int value)
        {
            Instance.SaveInt(key, value);
        }

        private void SaveInt(string key, int value)
        {
            cachedIntChanges[key] = value;
            PlayerPrefs.SetInt(key, value);
        }


        public static void SaveFloatSetting(string key, float value)
        {
            Instance.SaveFloat(key, value);
        }

        private void SaveFloat(string key, float value)
        {
            cachedFloatChanges[key] = value;
            PlayerPrefs.SetFloat(key, value);
        }

        public static void SaveStringSetting(string key, string value)
        {
            Instance.SaveString(key, value);
        }

        private void SaveString(string key, string value)
        {
            cachedStringChanges[key] = value;
            PlayerPrefs.SetString(key, value);
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
                PlayerPrefs.SetInt(pair.Key, pair.Value);
            }

            foreach (var pair in floatSettings)
            {
                cachedFloatChanges[pair.Key] = pair.Value;
                PlayerPrefs.SetFloat(pair.Key, pair.Value);
            }

            foreach (var pair in stringSettings)
            {
                cachedStringChanges[pair.Key] = pair.Value;
                PlayerPrefs.SetString(pair.Key, pair.Value);
            }

            PlayerPrefs.Save();
        }
    }
}