using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.SO.Settings.Screen;

namespace Editor.CommandCenter.Modules.Settings
{
    public class EnumItemModule : ISettingsItemModule
    {
        public SettingsItemType Type => SettingsItemType.Enum;

        private static readonly Type[] EnumTypes;

        static EnumItemModule()
        {
            EnumTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsEnum && t.Namespace == "Utils.Enum.Settings")
                .ToArray();
        }

        public SettingsPageItem CreateInstance()
        {
            var item = ScriptableObject.CreateInstance<SettingsPageItemEnum>();
            item.itemType = SettingsItemType.Enum;
            item.settingName = "New Enum Setting";
            return item;
        }

        public VisualElement CreateUI(SettingsPageItem baseItem, Action onRemove)
        {
            var item = baseItem as SettingsPageItemEnum;

            var container = new VisualElement
            {
                style = { flexDirection = FlexDirection.Column, marginBottom = 6 }
            };

            var row1 = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            if (item)
            {
                var nameField = new TextField
                {
                    value = item.settingName,
                    style = { flexGrow = 1 }
                };
                nameField.RegisterValueChangedCallback(evt =>
                {
                    item.settingName = evt.newValue;
                    EditorUtility.SetDirty(item);
                });

                var removeBtn = new Button(onRemove) { text = "x", style = { width = 24, marginLeft = 6 } };

                row1.Add(nameField);
                row1.Add(removeBtn);
            }
            container.Add(row1);

            var row2 = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginTop = 4 } };

            var typeNames = EnumTypes.Select(t => t.Name).ToList();
            var currentTypeIndex = 0;
            if (item && !string.IsNullOrEmpty(item.enumTypeName))
            {
                var currentType = item.EnumType;
                currentTypeIndex = Array.IndexOf(EnumTypes, currentType);
                if (currentTypeIndex < 0) currentTypeIndex = 0;
            }

            var typeDropdown = new PopupField<string>(typeNames, currentTypeIndex);
            row2.Add(typeDropdown);

            var valueDropdown = CreateValueDropdown(item);
            row2.Add(valueDropdown);

            typeDropdown.RegisterValueChangedCallback(evt =>
            {
                if (!item) return;
                var selectedType = EnumTypes.FirstOrDefault(t => t.Name == evt.newValue);
                if (selectedType == null) return;

                item.enumTypeName = selectedType.AssemblyQualifiedName;
                item.value = null;
                EditorUtility.SetDirty(item);

                row2.Remove(valueDropdown);
                valueDropdown = CreateValueDropdown(item);
                row2.Add(valueDropdown);
            });

            container.Add(row2);

            return container;
        }

        private PopupField<string> CreateValueDropdown(SettingsPageItemEnum item)
        {
            if (!item || item.EnumType is not { IsEnum: true })
                return new PopupField<string>(new System.Collections.Generic.List<string>(), 0);

            var values = Enum.GetNames(item.EnumType).ToList();
            var selectedIndex = values.IndexOf(item.value ?? values.First());

            var dropdown = new PopupField<string>(values, selectedIndex);
            dropdown.RegisterValueChangedCallback(evt =>
            {
                item.value = evt.newValue;
                EditorUtility.SetDirty(item);
            });

            dropdown.style.marginLeft = 6;
            dropdown.style.flexGrow = 1;

            return dropdown;
        }
    }
}