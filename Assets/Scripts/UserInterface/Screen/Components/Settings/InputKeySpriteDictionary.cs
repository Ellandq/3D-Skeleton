namespace UserInterface.Screen.Components.Settings
{
    using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UserInterface.Screen.Components.Settings
{
    [Serializable]
    public class InputKeySpriteDictionary
    {
        [Serializable]
        public class Entry
        {
            public string key;
            public Sprite value;
        }

        [SerializeField]
        private List<Entry> entries = new();

        private Dictionary<string, Sprite> dictionary;

        private void EnsureDictionary()
        {
            if (dictionary != null)
                return;

            dictionary = entries
                .Where(e => !string.IsNullOrEmpty(e.key))
                .GroupBy(e => e.key)
                .ToDictionary(
                    g => g.Key,
                    g => g.Last().value
                );
        }

        public Sprite this[string key]
        {
            get
            {
                EnsureDictionary();
                return dictionary.GetValueOrDefault(key);
            }
            set
            {
                EnsureDictionary();

                dictionary[key] = value;

                var entry = entries.FirstOrDefault(e => e.key == key);

                if (entry != null)
                {
                    entry.value = value;
                }
                else
                {
                    entries.Add(new Entry
                    {
                        key = key,
                        value = value
                    });
                }
            }
        }

        public bool TryGetValue(string key, out Sprite sprite)
        {
            EnsureDictionary();
            return dictionary.TryGetValue(key, out sprite);
        }

        public bool ContainsKey(string key)
        {
            EnsureDictionary();
            return dictionary.ContainsKey(key);
        }

        public void Add(string key, Sprite value)
        {
            this[key] = value;
        }

        public void Remove(string key)
        {
            EnsureDictionary();

            dictionary.Remove(key);

            entries.RemoveAll(e => e.key == key);
        }

        public IEnumerable<KeyValuePair<string, Sprite>> Pairs
        {
            get
            {
                EnsureDictionary();
                return dictionary;
            }
        }

        public void OnAfterDeserialize()
        {
            dictionary = null;
        }
    }
}
}