using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Data.Settings.Screen;

namespace Editor.CommandCenter.Screens.Modules.Settings
{
    public class SettingsPageCategoryModule : ISettingsPageModule
    {
        private readonly SettingsPageCategorySO _category;
        private readonly System.Action<SettingsPageCategorySO> _onRemove;

        public SettingsPageCategoryModule(SettingsPageCategorySO category, System.Action<SettingsPageCategorySO> onRemove)
        {
            _category = category;
            _onRemove = onRemove;
        }

        public VisualElement CreateUI()
        {
            var root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginBottom = 8,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 6,
                    paddingBottom = 6,
                    backgroundColor = new Color(0.27f, 0.27f, 0.27f),
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6
                }
            };

            var header = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 6 }
            };

            var nameField = new TextField
            {
                value = _category.categoryName,
                style = { flexGrow = 1, fontSize = 16 }
            };
            nameField.RegisterValueChangedCallback(e => _category.categoryName = e.newValue);

            var removeBtn = new Button(() => _onRemove?.Invoke(_category))
            {
                text = "X",
                style =
                {
                    width = 26,
                    height = 22,
                    backgroundColor = new Color(0.5f, 0.2f, 0.2f),
                    color = Color.white,
                    marginLeft = 6
                }
            };

            header.Add(nameField);
            header.Add(removeBtn);
            root.Add(header);

            foreach (var itemModule in _category.items.ToList().Select(item => new SettingsPageItemModule(item, removedItem =>
                     {
                         _category.items.Remove(removedItem);
                     })))
            {
                root.Add(itemModule.CreateUI());
            }

            var addItemBtn = new Button(() =>
            {
                var item = ScriptableObject.CreateInstance<SettingsPageItemSO>();

                item.settingName = "New Item";
                item.itemType = SettingsItemType.Boolean;
                item.BoolDefaultValue = false;

                _category.items.Add(item);

                var itemModule = new SettingsPageItemModule(item, removedItem =>
                {
                    _category.items.Remove(removedItem);
                });

                var insertIndex = root.childCount - 1;
                root.Insert(insertIndex, itemModule.CreateUI());
            })
            {
                text = "+ Add Item",
                style = { marginTop = 6, width = 100, height = 24 }
            };

            root.Add(addItemBtn);

            return root;
        }
    }
}