using System;
using System.Collections.Generic;
using System.Linq;
using GameInput;
using UnityEngine;

namespace Utils.Data.Settings
{
    namespace Utils.SO.Settings
    {
        [CreateAssetMenu(menuName = "Settings/Input Assignments")]
        public class InputAssignments : ScriptableObject
        {
            public List<InputAssignmentEntry> assignments = new();

            public Dictionary<string, string> AsDictionary()
            {
                var dict = new Dictionary<string, string>();

                foreach (var assignment in assignments)
                {
                    dict[assignment.settingName] = assignment.baseValue;
                    dict[assignment.settingName + "_Alt"] = assignment.altValue;
                }

                return dict;
            }

            public Dictionary<PlayerAction, (string baseValue, string altValue)> AsSimpleDictionary()
            {
                return assignments
                    .Where(x => x.action != default)
                    .ToDictionary(
                        x => x.action,
                        x => (x.baseValue, x.altValue)
                    );
            }

            public Dictionary<string, PlayerAction> ActionDictionary()
            {
                return assignments
                    .Where(x => x.action != default)
                    .ToDictionary(
                        x => x.settingName,
                        x => x.action
                    );
            }

            public void ApplyDictionary(Dictionary<string, string> dict)
            {
                foreach (var assignment in assignments)
                {
                    if (dict.TryGetValue(
                            assignment.settingName,
                            out var baseValue))
                    {
                        assignment.baseValue = baseValue;
                    }

                    if (dict.TryGetValue(
                            assignment.settingName + "_Alt",
                            out var altValue))
                    {
                        assignment.altValue = altValue;
                    }
                }
            }
        }
    }

    
    [Serializable]
    public class InputAssignmentEntry
    {
        public string settingName;
        public PlayerAction action = PlayerAction.Action1;

        public string baseValue = "";
        public string altValue = "";
    }
    
}