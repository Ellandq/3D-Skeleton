using System.Globalization;
using GameInput;
using UnityEngine;

namespace Utils.SO.Settings.Screen
{
    public class SettingsPageItemSO : ScriptableObject
    {
        public string uniqueId;
        public string settingName;
        public SettingsItemType itemType;
        public string strValue;

        // Float
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float MinIncrement { get; set; }
        public float FloatDefaultValue { get; set; }

        // Enum
        public string EnumTypeName { get; set; }
        public string EnumDefaultValue { get; set; }

        // Boolean
        public bool BoolDefaultValue { get; set; }

        // InputKey
        public PlayerAction ActionName { get; set; }
        
        // =========================
        // SERIALIZATION
        // =========================

        public void ConvertToString()
        {
            strValue = itemType switch
            {
                SettingsItemType.Float => string.Join("/", "float", FloatDefaultValue.ToString(CultureInfo.InvariantCulture),
                    MinValue.ToString(CultureInfo.InvariantCulture), MaxValue.ToString(CultureInfo.InvariantCulture),
                    MinIncrement.ToString(CultureInfo.InvariantCulture)),
                SettingsItemType.Enum => $"enum/{EnumTypeName}/{EnumDefaultValue}",
                SettingsItemType.Boolean => $"boolean/{BoolDefaultValue}",
                SettingsItemType.InputKey => $"inputKey/{ActionName}",
                _ => strValue
            };
        }

        public void ConvertFromString()
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var parts = strValue.Split('/');

            if (parts.Length == 0)
                return;

            switch (parts[0])
            {
                case "float":
                    itemType = SettingsItemType.Float;

                    if (parts.Length >= 5)
                    {
                        if (TryParse(parts[1], out var v)) FloatDefaultValue = v;
                        if (TryParse(parts[2], out var min)) MinValue = min;
                        if (TryParse(parts[3], out var max)) MaxValue = max;
                        if (TryParse(parts[4], out var inc)) MinIncrement = inc;
                    }
                    break;

                case "enum":
                    itemType = SettingsItemType.Enum;

                    if (parts.Length >= 3)
                    {
                        EnumTypeName = parts[1];
                        EnumDefaultValue = parts[2];
                    }
                    break;

                case "boolean":
                    itemType = SettingsItemType.Boolean;

                    if (parts.Length >= 2 && bool.TryParse(parts[1], out var b))
                        BoolDefaultValue = b;
                    break;

                case "inputKey":
                    itemType = SettingsItemType.InputKey;

                    if (parts.Length >= 2 &&
                        System.Enum.TryParse(parts[1], out PlayerAction action))
                        ActionName = action;
                    break;
            }
        }

        private static bool TryParse(string input, out float value)
        {
            return float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }

    public enum SettingsItemType
    {
        Float,
        Enum,
        Boolean,
        InputKey
    }
}