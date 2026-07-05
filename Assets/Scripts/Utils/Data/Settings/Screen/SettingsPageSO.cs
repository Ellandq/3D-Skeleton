using System.Collections.Generic;
using UnityEngine;

namespace Utils.Data.Settings.Screen
{
    [CreateAssetMenu(menuName = "Settings/Screen/Page")]
    public class SettingsPageSO : ScriptableObject
    {
        public string uniqueId;
        public int index;
        public string pageName;
        public List<SettingsPageCategorySO> categories = new();
    }
}