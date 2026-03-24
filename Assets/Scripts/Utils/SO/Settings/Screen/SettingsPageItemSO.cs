using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        public bool BooleanIsConditional { get; set; }
        [System.NonSerialized]
        public List<SettingsPageItemSO> ConditionalItems = new();

        // InputKey
        public PlayerAction ActionName { get; set; }
        
        // =========================
        // SERIALIZATION
        // =========================

        public void ConvertToString()
        {
            strValue = itemType switch
            {
                SettingsItemType.Float => string.Join("/",
                    "float",
                    FloatDefaultValue.ToString(CultureInfo.InvariantCulture),
                    MinValue.ToString(CultureInfo.InvariantCulture),
                    MaxValue.ToString(CultureInfo.InvariantCulture),
                    MinIncrement.ToString(CultureInfo.InvariantCulture)
                ),

                SettingsItemType.Enum => $"enum/{Escape(EnumTypeName)}/{Escape(EnumDefaultValue)}",

                SettingsItemType.Boolean => SerializeBoolean(),

                SettingsItemType.InputKey => $"inputKey/{Escape(ActionName.ToString())}",

                SettingsItemType.Custom => "custom",

                _ => strValue
            };
        }
        
        private string SerializeBoolean()
        {
            if (!BooleanIsConditional || ConditionalItems == null || ConditionalItems.Count == 0)
                return $"boolean/{BoolDefaultValue}/false";

            var children = ConditionalItems
                .Select(c =>
                {
                    c.ConvertToString();
                    return c.strValue.Replace("|", "");
                });

            return $"boolean/{BoolDefaultValue}/true/{string.Join("|", children)}";
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
                        EnumTypeName = Unescape(parts[1]);
                        EnumDefaultValue = Unescape(parts[2]);
                    }
                    break;

                case "boolean":
                    itemType = SettingsItemType.Boolean;

                    if (parts.Length >= 2 && bool.TryParse(parts[1], out var b))
                        BoolDefaultValue = b;

                    if (parts.Length >= 3 && bool.TryParse(parts[2], out var conditional))
                        BooleanIsConditional = conditional;

                    ConditionalItems = new List<SettingsPageItemSO>();

                    if (BooleanIsConditional && parts.Length >= 4)
                    {
                        var childrenRaw = parts[3].Split('|');

                        foreach (var childStr in childrenRaw)
                        {
                            var child = CreateInstance<SettingsPageItemSO>();
                            child.strValue = Unescape(childStr);
                            child.ConvertFromString();
                            ConditionalItems.Add(child);
                        }
                    }
                    break;

                case "inputKey":
                    itemType = SettingsItemType.InputKey;
                    if (parts.Length >= 2 && System.Enum.TryParse(Unescape(parts[1]), out PlayerAction action))
                        ActionName = action;
                    break;
                
                case "custom":
                    itemType = SettingsItemType.Custom;
                    break;
            }
        }
        
        private static string Escape(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("\\", @"\\")
                .Replace("/", "\\/")
                .Replace("|", "\\|");
        }

        private static string Unescape(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("\\|", "|")
                .Replace("\\/", "/")
                .Replace(@"\\", "\\");
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
        InputKey,
        Custom
    }
}