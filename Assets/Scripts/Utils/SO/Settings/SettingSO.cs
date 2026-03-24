using System.Collections.Generic;
using UnityEngine;

namespace Utils.SO.Settings
{
    public class SettingSO<T> : ScriptableObject
    {
        public List<string> settingNames;
        public List<T> defaultValues;
    }
}