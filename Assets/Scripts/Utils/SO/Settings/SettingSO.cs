using System.Collections.Generic;
using GameInput;
using UnityEngine;

namespace Utils.SO.Settings
{
    public class SettingSO<T> : ScriptableObject
    {
        public List<string> settingNames;
        public List<T> defaultValues;
    }
}