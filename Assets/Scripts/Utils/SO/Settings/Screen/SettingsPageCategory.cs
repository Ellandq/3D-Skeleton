using System.Collections.Generic;
using UnityEngine;

namespace Utils.SO.Settings.Screen
{
    [CreateAssetMenu(menuName = "Settings/Screen/Category")]
    public class SettingsPageCategory : ScriptableObject
    {
        [Header("Category Info")] 
        public string categoryName;

        [Header("Items")] 
        public List<SettingsPageItem> items = new();
    }
}