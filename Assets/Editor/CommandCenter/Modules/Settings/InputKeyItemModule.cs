using System;
using System.Linq;
using GameInput;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.SO.Settings.Screen;
using Utils.Enum.Settings;

namespace Editor.CommandCenter.Modules.Settings
{
    public class InputKeyItemModule : ISettingsItemModule
    {
        public SettingsItemType Type => SettingsItemType.InputKey;

        public SettingsPageItem CreateInstance()
        {
            var item = ScriptableObject.CreateInstance<SettingsPageItemInputKey>();
            item.itemType = SettingsItemType.InputKey;
            item.settingName = "New Input Key";
            item.actionName = PlayerAction.Action1;
            return item;
        }

        public VisualElement CreateUI(SettingsPageItem baseItem, Action onRemove)
        {
            var item = baseItem as SettingsPageItemInputKey;

            var container = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 6 }
            };

            if (!item)
                return container;
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

            var actions = Enum.GetNames(typeof(PlayerAction)).ToList();
            var selectedIndex = actions.IndexOf(item.actionName.ToString());
            var actionDropdown = new PopupField<string>(actions, selectedIndex)
            {
                style = { width = 120, marginLeft = 6 }
            };

            actionDropdown.RegisterValueChangedCallback(evt =>
            {
                if (!Enum.TryParse<PlayerAction>(evt.newValue, out var parsed))
                    return;
                item.actionName = parsed;
                EditorUtility.SetDirty(item);
            });

            var removeBtn = new Button(onRemove) { text = "x", style = { width = 24, marginLeft = 6 } };

            container.Add(nameField);
            container.Add(actionDropdown);
            container.Add(removeBtn);

            return container;
        }
    }
}