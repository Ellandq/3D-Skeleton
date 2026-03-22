using System.Collections.Generic;
using UnityEngine;

namespace Utils.SO.Settings.Screen
{
    [CreateAssetMenu(menuName = "Settings/Screen/Page")]
    public class SettingsPageSO : ScriptableObject
    {
        public List<SettingsPageCategory> categories;
    }
}