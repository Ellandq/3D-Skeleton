using System;
using GameInput;
using UnityEngine;

namespace Utils.SO.Settings.Screen
{
    public abstract class SettingsPageItem : ScriptableObject
    {
        public string settingName;
        public SettingsItemType itemType;
        public string strValue;
    }

    [CreateAssetMenu(menuName = "Settings/Screen/Item/Float")]
    public class SettingsPageItemFloat : SettingsPageItem
    {
        public float minValue;
        public float maxValue;
        public float minIncrement;
        public float value;
    }

    [CreateAssetMenu(menuName = "Settings/Screen/Enum")]
    public class SettingsPageItemEnum : SettingsPageItem
    {
        public string enumTypeName;
        public Type EnumType => Type.GetType(enumTypeName);
        public string value;
    }

    [CreateAssetMenu(menuName = "Settings/Screen/Boolean")]
    public class SettingsPageItemBoolean : SettingsPageItem
    {
        public bool value;
    }

    [CreateAssetMenu(menuName = "Settings/Screen/InputKey")]
    public class SettingsPageItemInputKey : SettingsPageItem
    {
        public PlayerAction actionName;
    }

    public enum SettingsItemType
    {
        Numeral,
        Enum,
        Boolean,
        InputKey
    }
}