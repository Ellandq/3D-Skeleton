using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Data.Settings.Screen;

namespace Editor.CommandCenter.Screens.Modules.Settings
{
    public class SettingsPageModule : ISettingsPageModule
    {
        private readonly SettingsPageSO _page;

        private readonly Action<SettingsPageSO> _onRemove;
        private readonly Action _onChanged;

        public SettingsPageModule(SettingsPageSO page, Action<SettingsPageSO> onRemove, Action onChanged)
        {
            _page = page;
            _onRemove = onRemove;
            _onChanged = onChanged;
        }

        public VisualElement CreateUI()
        {
            var root = new ScrollView { style = { flexGrow = 1 } };

            var topBar = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, marginBottom = 6, alignItems = Align.Center }
            };

            var nameField = new TextField
            {
                value = _page.pageName,
                style = { flexGrow = 1, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold }
            };
            nameField.RegisterValueChangedCallback(e =>
            {
                _page.pageName = e.newValue;
                _onChanged?.Invoke();
            });
            topBar.Add(nameField);

            var removeBtn = new Button(() =>
            {
                if (_page is not null)
                {
                    _onRemove?.Invoke(_page);
                }
            })
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
            topBar.Add(removeBtn);

            root.Add(topBar);

            foreach (var catModule in _page.categories.Select(c => new SettingsPageCategoryModule(c, cat =>
                     {
                         _page.categories.Remove(cat);
                         root.Clear();
                         root.Add(CreateUI());
                     })))
            {
                root.Add(catModule.CreateUI());
            }

            var addBtn = new Button(() =>
            {
                var cat = ScriptableObject.CreateInstance<SettingsPageCategorySO>();
                cat.uniqueId = GUID.Generate().ToString();
                cat.categoryName = "New Category";
                cat.items = new List<SettingsPageItemSO>();
                _page.categories.Add(cat);

                root.Clear();
                root.Add(CreateUI());
            })
            { text = "+ Add Category", style = { marginTop = 8, width = 120, height = 28 } };

            root.Add(addBtn);

            return root;
        }
    }
}