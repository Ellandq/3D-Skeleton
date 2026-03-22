using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.SO.Settings.Screen;

namespace Editor.CommandCenter.Modules.Settings
{
    public class BooleanItemModule : ISettingsItemModule
    {
        public SettingsItemType Type => SettingsItemType.Boolean;

        public SettingsPageItem CreateInstance()
        {
            var item = ScriptableObject.CreateInstance<SettingsPageItemBoolean>();
            item.itemType = SettingsItemType.Boolean;
            item.settingName = "New Boolean Setting";
            item.value = false;
            return item;
        }

        public VisualElement CreateUI(SettingsPageItem baseItem, Action onRemove)
        {
            var item = baseItem as SettingsPageItemBoolean;

            var container = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 6 }
            };

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

            var toggle = new Toggle
            {
                value = item.value,
                style = { width = 50, marginLeft = 6 }
            };

            toggle.RegisterValueChangedCallback(evt =>
            {
                item.value = evt.newValue;
                EditorUtility.SetDirty(item);
            });

            var removeBtn = new Button(onRemove)
            {
                text = "x",
                style = { width = 24, marginLeft = 6 }
            };

            container.Add(nameField);
            container.Add(toggle);
            container.Add(removeBtn);

            return container;
        }
    }
}