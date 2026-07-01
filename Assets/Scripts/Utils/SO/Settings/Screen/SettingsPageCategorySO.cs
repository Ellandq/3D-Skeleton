using System.Collections.Generic;
using UnityEngine;

namespace Utils.SO.Settings.Screen
{
    [CreateAssetMenu(menuName = "Settings/Screen/Category")]
    public class SettingsPageCategorySO : ScriptableObject
    {
        public string uniqueId;
        public string categoryName;
        public List<SettingsPageItemSO> items = new();
    }
}