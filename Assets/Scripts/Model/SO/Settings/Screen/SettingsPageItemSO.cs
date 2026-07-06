using System.Collections.Generic;
using System.Globalization;
using Model.Enum.GameInput;
using UnityEngine;

namespace Model.SO.Settings.Screen
{
    public class SettingsPageItemSO : ScriptableObject
    {
        public string uniqueId;
        public string settingName;
        public string fullName;
        public SettingsItemType itemType;
        public string strValue;

        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float MinIncrement { get; set; }
        public float FloatDefaultValue { get; set; }

        public string EnumTypeName { get; set; }
        public string EnumDefaultValue { get; set; }

        public bool BoolDefaultValue { get; set; }
        public bool BooleanIsConditional { get; set; }
        [System.NonSerialized]
        public List<SettingsPageItemSO> ConditionalItems = new();

        public PlayerAction ActionName { get; set; }
        public bool AllowSecondaryInput { get; set; } = true;

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

                SettingsItemType.InputKey =>
                    string.Join("/",
                        "inputKey",
                        Escape(ActionName.ToString()),
                        AllowSecondaryInput.ToString()
                    ),

                SettingsItemType.Custom => "custom",

                _ => strValue
            };
        }
        
        private string SerializeBoolean()
        {
            var data = new BooleanData
            {
                value = BoolDefaultValue,
                conditional = BooleanIsConditional,
                children = new List<BooleanChildData>()
            };

            if (!BooleanIsConditional || ConditionalItems == null)
                return "boolean/" + JsonUtility.ToJson(data);
            foreach (var child in ConditionalItems)
            {
                child.ConvertToString();
                data.children.Add(new BooleanChildData
                {
                    settingName = child.settingName,
                    strValue = child.strValue
                });
            }

            return "boolean/" + JsonUtility.ToJson(data);
        }

        public void ConvertFromString()
        {
            if (string.IsNullOrEmpty(strValue))
                return;
            
            var typeSeparatorIndex = strValue.IndexOf('/');

            if (typeSeparatorIndex == -1)
                return;

            var type = strValue[..typeSeparatorIndex];
            var payload = strValue[(typeSeparatorIndex + 1)..];

            switch (type)
            {
                case "float":
                    itemType = SettingsItemType.Float;

                    var floatParts = payload.Split('/');

                    if (floatParts.Length >= 4)
                    {
                        if (TryParse(floatParts[0], out var v)) FloatDefaultValue = v;
                        if (TryParse(floatParts[1], out var min)) MinValue = min;
                        if (TryParse(floatParts[2], out var max)) MaxValue = max;
                        if (TryParse(floatParts[3], out var inc)) MinIncrement = inc;
                    }
                    break;

                case "enum":
                    itemType = SettingsItemType.Enum;

                    var enumParts = payload.Split('/');

                    if (enumParts.Length >= 2)
                    {
                        EnumTypeName = Unescape(enumParts[0]);
                        EnumDefaultValue = Unescape(enumParts[1]);
                    }
                    break;

                case "boolean":
                    itemType = SettingsItemType.Boolean;

                    var data = JsonUtility.FromJson<BooleanData>(payload);
                    if (data == null) return;

                    BoolDefaultValue = data.value;
                    BooleanIsConditional = data.conditional;

                    ConditionalItems = new List<SettingsPageItemSO>();
                    if (BooleanIsConditional && data.children != null)
                    {
                        foreach (var childData in data.children)
                        {
                            var child = CreateInstance<SettingsPageItemSO>();
                            child.settingName = childData.settingName;
                            child.strValue = childData.strValue;
                            child.ConvertFromString();
                            ConditionalItems.Add(child);
                        }
                    }
                    break;

                case "inputKey":
                    itemType = SettingsItemType.InputKey;

                    var parts = payload.Split('/');

                    if (parts.Length >= 1 &&
                        System.Enum.TryParse(Unescape(parts[0]), out PlayerAction action))
                    {
                        ActionName = action;
                    }

                    if (parts.Length >= 2)
                    {
                        bool.TryParse(parts[1], out var allowSecondary);
                        AllowSecondaryInput = allowSecondary;
                    }
                    else
                    {
                        AllowSecondaryInput = true;
                    }

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
        
        [System.Serializable]
        private class BooleanChildData
        {
            public string settingName;
            public string strValue;
        }

        [System.Serializable]
        private class BooleanData
        {
            public bool value;
            public bool conditional;
            public List<BooleanChildData> children;
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