using System.Collections.Generic;
using System.Linq;
using GameInput;
using UnityEngine;

namespace Utils.SO.Settings
{
    public class InputAssignments : SettingSO
    {
        public Dictionary<string, string> AsDictionary() => settingNames
                .Select((setting, index) => new
                {
                    Setting = setting,
                    Value = defaultValues[index]
                })
                .ToDictionary(kvp => kvp.Setting, kvp => kvp.Value);

        public void FromDictionary(Dictionary<string, string> dict)
        {
            settingNames.Clear();
            defaultValues.Clear();

            foreach (var kvp in dict)
            {
                settingNames.Add(kvp.Key);
                defaultValues.Add(kvp.Value);
            }
        }
        
        public Dictionary<PlayerAction, (string baseValue, string altValue)> AsSimpleDictionary()
        {
            var dict = AsDictionary();

            var simpleDict = new Dictionary<PlayerAction, (string baseValue, string altValue)>();

            foreach (var (key, baseValue) in dict)
            {
                if (key.EndsWith("_Alt"))
                    continue;

                dict.TryGetValue(key + "_Alt", out var altValue);

                System.Enum.TryParse<PlayerAction>(key, out var enumKey);

                simpleDict[enumKey] = (baseValue, altValue);
            }

            return simpleDict;
        }
    }
}