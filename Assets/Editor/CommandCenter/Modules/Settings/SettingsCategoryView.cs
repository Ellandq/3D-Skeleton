using System;
using System.Linq;
using Editor.CommandCenter.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.SO.Settings.Screen;

namespace Editor.CommandCenter.Modules.Settings
{
    public static class SettingsCategoryView
    {
        public static VisualElement Create(
            SettingsPageCategory category,
            Action<SettingsPageCategory> onRemoveCategory,
            Action<SettingsPageCategory, SettingsPageItem> onRemoveItem,
            Action<SettingsPageCategory, SettingsPageItem, SettingsItemType> onChangeType)
        {
            var root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginBottom = 12,
                    backgroundColor = new Color(0.18f, 0.18f, 0.18f)
                }
            };

            #region HEADER

            var header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginBottom = 8,
                    alignItems = Align.Center
                }
            };

            var nameField = new TextField
            {
                value = category.categoryName,
                style =
                {
                    flexGrow = 1,
                    fontSize = 14,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };

            nameField.RegisterValueChangedCallback(evt =>
            {
                category.categoryName = evt.newValue;
            });

            var delete = new Button(() => onRemoveCategory(category))
            {
                text = "x"
            };

            delete.style.width = 24;
            delete.style.marginLeft = 6;

            header.Add(nameField);
            header.Add(delete);

            #endregion
            
            var separator = new VisualElement
            {
                style =
                {
                    height = 1,
                    backgroundColor = new Color(0.1f, 0.1f, 0.1f),
                    marginBottom = 6
                }
            };

            #region ITEMS

            var itemsContainer = new VisualElement();

            foreach (var item in category.items.ToList())
            {
                var module = SettingsItemRegistry.Get(item.itemType);
                if (module == null)
                    continue;

                var wrapper = new VisualElement
                {
                    style = { marginBottom = 6 }
                };

                var typeDropdown = new EnumField(item.itemType)
                {
                    style =
                    {
                        alignSelf = Align.FlexEnd
                    }
                };

                typeDropdown.RegisterValueChangedCallback(evt =>
                {
                    onChangeType(category, item, (SettingsItemType)evt.newValue);
                });

                var ui = module.CreateUI(item, () => onRemoveItem(category, item));

                wrapper.Add(typeDropdown);
                wrapper.Add(ui);

                itemsContainer.Add(wrapper);
            }

            #endregion
            
            var addItemBtn = new Button(() =>
            {
                var module = SettingsItemRegistry.Get(SettingsItemType.Numeral);
                if (module == null)
                    return;

                var item = module.CreateInstance();
                category.items.Add(item);

                onChangeType?.Invoke(category, item, item.itemType);
            })
            {
                text = "+ Add Item",
                style =
                {
                    marginTop = 8,
                    marginBottom = 12,
                    alignSelf = Align.Center,
                    width = 120,
                    height = 28
                }
            };

            root.Add(header);
            root.Add(separator);
            root.Add(itemsContainer);
            root.Add(addItemBtn);

            return root;
        }
    }
}