using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.SO.Settings.Screen;

namespace Editor.CommandCenter.Modules.Settings
{
    public class FloatItemModule : ISettingsItemModule
    {
        public SettingsItemType Type => SettingsItemType.Numeral;

        public SettingsPageItem CreateInstance()
        {
            var item = ScriptableObject.CreateInstance<SettingsPageItemFloat>();
            item.itemType = SettingsItemType.Numeral;
            item.settingName = "New Float Setting";
            return item;
        }

        public VisualElement CreateUI(SettingsPageItem baseItem, Action onRemove)
        {
            var item = baseItem as SettingsPageItemFloat;

            var root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginBottom = 8,
                    paddingBottom = 6,
                    borderBottomWidth = 1,
                    borderBottomColor = new Color(0.1f, 0.1f, 0.1f)
                }
            };

            #region FIRST ROW

            var row1 = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };

            var nameField = new TextField
            {
                value = item.settingName
            };
            nameField.style.flexGrow = 1;

            nameField.RegisterValueChangedCallback(evt =>
            {
                item.settingName = evt.newValue;
                EditorUtility.SetDirty(item);
            });

            var incrementField = new FloatField
            {
                value = item.minIncrement
            };
            incrementField.style.width = 70;
            incrementField.style.marginLeft = 6;

            incrementField.RegisterValueChangedCallback(evt =>
            {
                item.minIncrement = evt.newValue;
                EditorUtility.SetDirty(item);
            });

            var removeBtn = new Button(onRemove)
            {
                text = "x"
            };
            removeBtn.style.width = 24;
            removeBtn.style.marginLeft = 6;

            row1.Add(nameField);
            row1.Add(incrementField);
            row1.Add(removeBtn);

            #endregion

            #region SECOND ROW

            var row2 = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginTop = 4
                }
            };

            var minField = new FloatField
            {
                value = item.minValue
            };
            minField.style.width = 60;

            var slider = new Slider
            {
                lowValue = item.minValue,
                highValue = item.maxValue,
                value = item.value
            };
            slider.style.flexGrow = 1;
            slider.style.marginLeft = 6;
            slider.style.marginRight = 6;

            slider.RegisterValueChangedCallback(evt =>
            {
                item.value = evt.newValue;
                EditorUtility.SetDirty(item);
            });

            var maxField = new FloatField
            {
                value = item.maxValue
            };
            maxField.style.width = 60;

            maxField.RegisterValueChangedCallback(evt =>
            {
                item.maxValue = evt.newValue;
                EditorUtility.SetDirty(item);
                slider.highValue = item.maxValue;
            });

            row2.Add(minField);
            row2.Add(slider);
            row2.Add(maxField);

            #endregion

            root.Add(row1);
            root.Add(row2);

            return root;
        }
    }
}