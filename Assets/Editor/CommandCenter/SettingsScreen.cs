using System.Collections.Generic;
using System.Linq;
using Editor.CommandCenter.Modules.Settings;
using Editor.CommandCenter.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.SO.Settings.Screen;

namespace Editor.CommandCenter
{
    public class SettingsScreen : ICommandCenterScreen
    {
        public string ScreenName => "Settings";

        private ICommandCenterLogger _logger;

        private readonly List<SettingsPageSO> _pages = new();

        private ScrollView _pageButtonScroll;
        private VisualElement _pageButtonContainer;

        private VisualElement _contentContainer;

        private SettingsPageSO _activePage;

        public void Initialize(ICommandCenterLogger logger)
        {
            _logger = logger;
            LoadPages();
        }

        public VisualElement CreateContent()
        {
            var root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    flexGrow = 1,
                    paddingLeft = 12,
                    paddingRight = 12,
                    paddingTop = 10,
                    paddingBottom = 10
                }
            };

            CreateHeader(root);
            CreateContentArea(root);

            return root;
        }

        public void OnShow() { }
        public void OnHide() { }

        #region HEADER

        private void CreateHeader(VisualElement root)
        {
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginBottom = 10,
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f),
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6,
                }
            };

            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            var addButton = new Button(CreateNewPage)
            {
                text = "+",
                style =
                {
                    width = 32,
                    height = 28,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginRight = 6
                }
            };

            _pageButtonScroll = new ScrollView(ScrollViewMode.Horizontal)
            {
                style = { flexGrow = 1 }
            };

            _pageButtonContainer = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row }
            };

            _pageButtonScroll.Add(_pageButtonContainer);

            row.Add(addButton);
            row.Add(_pageButtonScroll);

            var separator = new VisualElement
            {
                style =
                {
                    height = 1,
                    backgroundColor = new Color(0.1f, 0.1f, 0.1f),
                    marginTop = 6
                }
            };

            container.Add(row);
            container.Add(separator);

            root.Add(container);

            RefreshPageButtons();
        }

        private void RefreshPageButtons()
        {
            _pageButtonContainer.Clear();

            foreach (var page in _pages)
            {
                var btn = new Button(() => SelectPage(page))
                {
                    text = page.name,
                    style =
                    {
                        marginRight = 6,
                        height = 28,
                        paddingLeft = 10,
                        paddingRight = 10
                    }
                };

                if (page == _activePage)
                    btn.style.backgroundColor = new Color(0.35f, 0.35f, 0.35f);

                _pageButtonContainer.Add(btn);
            }
        }

        #endregion

        #region CONTENT

        private void CreateContentArea(VisualElement root)
        {
            _contentContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    backgroundColor = new Color(0.18f, 0.18f, 0.18f)
                }
            };

            root.Add(_contentContainer);
        }

        private void SelectPage(SettingsPageSO page)
        {
            _activePage = page;
            DrawPage();
        }

        private void DrawPage()
        {
            _contentContainer.Clear();

            if (!_activePage)
                return;

            _activePage.categories ??= new List<SettingsPageCategory>();

            DrawTopBar();

            var scroll = new ScrollView
            {
                style =
                {
                    flexGrow = 1,
                    backgroundColor = new Color(0.22f, 0.22f, 0.22f),
                }
            };

            foreach (var category in _activePage.categories.ToList())
            {
                scroll.Add(SettingsCategoryView.Create(
                    category,
                    RemoveCategory,
                    RemoveItem,
                    ReplaceItemType));
            }

            DrawBottomButtons(scroll);

            _contentContainer.Add(scroll);
        }

        private void DrawTopBar()
        {
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginBottom = 12
                }
            };

            var nameField = new TextField
            {
                value = _activePage.name,
                style =
                {
                    fontSize = 18,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    height = 28
                }
            };

            nameField.RegisterValueChangedCallback(evt =>
            {
                _activePage.name = evt.newValue;
                EditorUtility.SetDirty(_activePage);
                RefreshPageButtons();
            });

            var separator = new VisualElement
            {
                style =
                {
                    height = 1,
                    backgroundColor = new Color(0.1f, 0.1f, 0.1f),
                    marginTop = 6
                }
            };

            container.Add(nameField);
            container.Add(separator);

            _contentContainer.Add(container);
        }

        private void DrawBottomButtons(VisualElement root)
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.Center,
                    marginTop = 10
                }
            };

            var addCategory = new Button(AddCategory)
            {
                text = "+",
                style =
                {
                    width = 36,
                    height = 36,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    backgroundColor = new Color(0.3f, 0.3f, 0.3f),
                    marginTop = 10
                }
            };

            row.Add(addCategory);

            row.Add(addCategory);

            root.Add(row);
        }

        #endregion

        #region CATEGORY + ITEM LOGIC

        private void AddCategory()
        {
            var cat = ScriptableObject.CreateInstance<SettingsPageCategory>();
            cat.categoryName = "New Category";
            cat.items = new List<SettingsPageItem>();

            _activePage.categories.Add(cat);

            DrawPage();
        }

        private void RemoveCategory(SettingsPageCategory category)
        {
            _activePage.categories.Remove(category);
            DrawPage();
        }

        private void RemoveItem(SettingsPageCategory category, SettingsPageItem item)
        {
            category.items.Remove(item);
            DrawPage();
        }

        private void ReplaceItemType(SettingsPageCategory category, SettingsPageItem oldItem, SettingsItemType newType)
        {
            var module = SettingsItemRegistry.Get(newType);
            if (module == null)
                return;

            var newItem = module.CreateInstance();
            newItem.settingName = oldItem.settingName;

            var index = category.items.IndexOf(oldItem);
            category.items[index] = newItem;

            DrawPage();
        }

        #endregion

        #region DATA

        private void LoadPages()
        {
            _pages.Clear();

            var guids = AssetDatabase.FindAssets("t:SettingsPageSO");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<SettingsPageSO>(path);
                if (asset)
                    _pages.Add(asset);
            }
        }

        private void CreateNewPage()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Settings Page", "SettingsPage", "asset", "");
            if (string.IsNullOrEmpty(path)) return;

            var page = ScriptableObject.CreateInstance<SettingsPageSO>();
            AssetDatabase.CreateAsset(page, path);
            AssetDatabase.SaveAssets();

            _pages.Add(page);
            RefreshPageButtons();
            SelectPage(page);
        }

        #endregion
    }
}