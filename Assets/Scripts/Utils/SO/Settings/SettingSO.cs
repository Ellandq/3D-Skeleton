using System.Collections.Generic;
using GameInput;
using UnityEngine;

namespace Utils.SO.Settings
{
    public class SettingSO : ScriptableObject
    {
        public List<string> settingNames;
        public List<string> defaultValues;
    }
}