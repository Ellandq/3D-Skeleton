using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils.SO.Input
{
    [CreateAssetMenu(
        fileName = "AllowedInputKeys",
        menuName = "Input/Allowed Input Keys")]
    public class AllowedInputKeys : ScriptableObject
    {
        public List<InputKeyCategory> categories = new();
    }


    [Serializable]
    public class InputKeyCategory
    {
        public InputKeyType type;
        public string categoryName;
        public List<InputKeyEntry> keys = new();
    }


    [Serializable]
    public class InputKeyEntry
    {
        public string value;

        public int intValue;

        public bool isAllowed = true;
    }


    public enum InputKeyType
    {
        MouseKey,
        KeyCode
    }
}