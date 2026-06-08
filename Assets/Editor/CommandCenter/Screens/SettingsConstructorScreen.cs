using System.Collections.Generic;
using System.Linq;
using Editor.CommandCenter.Screens.Modules.Settings;
using Editor.CommandCenter.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.SO.Settings.Screen;

namespace Editor.CommandCenter.Screens
{
    public class SettingsConstructorScreen : ICommandCenterScreen
    {
        public string ScreenName => "Settings";

        private ICommandCenterLogger _logger;

        private ScrollView _pageButtonScroll;
        private VisualElement _pageButtonContainer;
        private VisualElement _contentContainer;

        private Dictionary<string, Dictionary<string, List<string>>> _savedAssetKeys = new();
        private List<SettingsPageSO> _pages = new();

        private SettingsPageSO _activePage;

        private const string Root = "Assets/ScriptableObjects/Settings/Pages";

        public void Initialize(ICommandCenterLogger logger)
        {
            _logger = logger;
            LoadPagesFromDisk();
        }

        #region UI

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
                    paddingTop = 6,
                    paddingBottom = 6,
                    paddingLeft = 8,
                    paddingRight = 8
                }
            };

            var title = new Label("Pages:")
            {
                style =
                {
                    fontSize = 14,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 6
                }
            };

            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };

            var leftGroup = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1
                }
            };

            var addButton = new Button(AddPage)
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
            leftGroup.Add(addButton);
            leftGroup.Add(_pageButtonScroll);

            var saveButton = new Button(SaveAll)
            {
                text = "SAVE",
                style =
                {
                    width = 80,
                    height = 30,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    backgroundColor = new Color(0.2f, 0.5f, 0.25f),
                    color = Color.white,
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4
                }
            };

            row.Add(leftGroup);
            row.Add(saveButton);

            container.Add(title);
            container.Add(row);
            root.Add(container);

            RefreshPageButtons();
        }

        private void CreateContentArea(VisualElement root)
        {
            _contentContainer = new ScrollView
            {
                style =
                {
                    flexGrow = 1,
                    marginTop = 8,
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 10,
                    paddingBottom = 10,
                    backgroundColor = new Color(0.22f, 0.22f, 0.22f),
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6,
                }
            };

            root.Add(_contentContainer);
        }

        private void RefreshPageButtons()
        {
            _pageButtonContainer.Clear();

            for (var i = 0; i < _pages.Count; i++)
            {
                var page = _pages[i];
                var index = i;

                var btnSetContainer = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        marginRight = 4,
                        paddingLeft = 2,
                        paddingRight = 2,
                        height = 28,
                        borderTopLeftRadius = 4,
                        borderTopRightRadius = 4,
                        borderBottomLeftRadius = 4,
                        borderBottomRightRadius = 4,
                        backgroundColor = new Color(0.15f, 0.15f, 0.15f)
                    }
                };

                var leftBtn = new Button(() => MovePageLeft(index)) { text = "<", style = { width = 20, height = 20, marginRight = 2 } };
                leftBtn.SetEnabled(index > 0);
                btnSetContainer.Add(leftBtn);

                var btn = new Button(() => SelectPage(page)) { text = page.pageName, style = { flexGrow = 1, height = 20 } };
                if (page == _activePage)
                    btn.style.backgroundColor = new Color(0.35f, 0.35f, 0.35f);
                btnSetContainer.Add(btn);

                var rightBtn = new Button(() => MovePageRight(index)) { text = ">", style = { width = 20, height = 20, marginLeft = 2 } };
                rightBtn.SetEnabled(index < _pages.Count - 1);
                btnSetContainer.Add(rightBtn);

                _pageButtonContainer.Add(btnSetContainer);
            }
        }
        
        private void MovePageLeft(int index)
        {
            if (index <= 0) return;

            var page = _pages[index];
            _pages.RemoveAt(index);
            _pages.Insert(index - 1, page);

            UpdatePageIndices();
            RefreshPageButtons();
        }

        private void MovePageRight(int index)
        {
            if (index >= _pages.Count - 1) return;

            var page = _pages[index];
            _pages.RemoveAt(index);
            _pages.Insert(index + 1, page);

            UpdatePageIndices();
            RefreshPageButtons();
        }

        private void SelectPage(SettingsPageSO page)
        {
            _activePage = page;
            DrawPage();
            RefreshPageButtons();
        }

        private void DrawPage()
        {
            _contentContainer.Clear();
            if (!_activePage) return;

            var pageModule = new SettingsPageModule(
                _activePage,
                page =>
                {
                    _pages.Remove(page);
                    _activePage = null;
                    _contentContainer.Clear();
                    RefreshPageButtons();
                },
                RefreshPageButtons
            );
            _contentContainer.Add(pageModule.CreateUI());
        }

        #endregion

        #region LOAD

        private void LoadPagesFromDisk()
        {
            _pages = AssetDatabase.FindAssets("t:SettingsPageSO", new[] { Root })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SettingsPageSO>)
                .Where(a => a)
                .OrderBy(p => p.index)
                .ToList();

            foreach (var page in _pages)
            {
                page.categories ??= new List<SettingsPageCategorySO>();

                foreach (var cat in page.categories)
                {
                    cat.items ??= new List<SettingsPageItemSO>();
                }
            }
            
            UpdateKeyDictionary();
        }

        private void UpdateKeyDictionary()
        {
            _savedAssetKeys = _pages
                .ToDictionary(
                    page => page.uniqueId,
                    page => page.categories.ToDictionary(
                        category => category.uniqueId,
                        category => category.items.Select(item => item.uniqueId).ToList()
                    )
                );
        }

        #endregion

        #region SAVE

        private void SaveAll()
        {
            foreach (var page in _pages)
                SavePageWithChildren(page);

            RemoveOrphanedAssets();
            
            GenerateCustomEnum();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            UpdateKeyDictionary();
        }

        private void SavePageWithChildren(SettingsPageSO page)
        {
            if (string.IsNullOrEmpty(page.uniqueId))
                page.uniqueId = GUID.Generate().ToString();
            page.name = page.uniqueId;

            var pagePath = $"{Root}/{page.uniqueId}.asset";
            var existingPage = AssetDatabase.LoadAssetAtPath<SettingsPageSO>(pagePath);
            if (!existingPage)
                AssetDatabase.CreateAsset(page, pagePath);
            else
                EditorUtility.CopySerialized(page, existingPage);

            var folder = GetPageFolder(page);
            EnsureFolder(folder);

            foreach (var cat in page.categories)
                SaveCategoryWithChildren(page, cat);
        }

        private void SaveCategoryWithChildren(SettingsPageSO page, SettingsPageCategorySO cat)
        {
            if (string.IsNullOrEmpty(cat.uniqueId))
                cat.uniqueId = GUID.Generate().ToString();
            cat.name = cat.uniqueId;

            var folder = GetPageFolder(page);
            var catPath = $"{folder}/{cat.uniqueId}.asset";
            var existingCat = AssetDatabase.LoadAssetAtPath<SettingsPageCategorySO>(catPath);
            if (!existingCat)
                AssetDatabase.CreateAsset(cat, catPath);
            else
                EditorUtility.CopySerialized(cat, existingCat);

            var catFolder = GetCategoryFolder(page, cat);
            EnsureFolder(catFolder);

            foreach (var item in cat.items)
                SaveItem(catFolder, item);
        }

        private void SaveItem(string folder, SettingsPageItemSO item)
        {
            if (string.IsNullOrEmpty(item.uniqueId))
                item.uniqueId = GUID.Generate().ToString();
            item.name = item.uniqueId;
            item.ConvertToString();
            var path = $"{folder}/{item.uniqueId}.asset";
            var existingItem = AssetDatabase.LoadAssetAtPath<SettingsPageItemSO>(path);
            if (!existingItem)
                AssetDatabase.CreateAsset(item, path);
            else
                EditorUtility.CopySerialized(item, existingItem);
        }

        private void RemoveOrphanedAssets()
        {
            var oldKeys = new Dictionary<string, Dictionary<string, List<string>>>(_savedAssetKeys);

            UpdateKeyDictionary();

            foreach (var (oldPageId, oldCategories) in oldKeys)
            {
                if (!_savedAssetKeys.TryGetValue(oldPageId, out var newCategories))
                {
                    var pagePath = $"{Root}/{oldPageId}.asset";
                    if (AssetDatabase.LoadAssetAtPath<SettingsPageSO>(pagePath))
                        AssetDatabase.DeleteAsset(pagePath);

                    var folder = $"{Root}/{oldPageId}";
                    DeleteFolderByPattern(folder);

                    continue;
                }

                var page = _pages.First(p => p.uniqueId == oldPageId);

                foreach (var (oldCatId, oldItems) in oldCategories)
                {
                    if (!newCategories.TryGetValue(oldCatId, out var newItems))
                    {
                        var catPath = $"{GetPageFolder(page)}/{oldCatId}.asset";
                        if (AssetDatabase.LoadAssetAtPath<SettingsPageCategorySO>(catPath))
                            AssetDatabase.DeleteAsset(catPath);

                        var catFolder = $"{GetPageFolder(page)}/{oldCatId}";
                        DeleteFolderByPattern(catFolder);

                        continue;
                    }

                    var cat = page.categories.First(c => c.uniqueId == oldCatId);

                    foreach (var itemPath in from oldItemId in oldItems
                             where !newItems.Contains(oldItemId)
                             select $"{GetCategoryFolder(page, cat)}/{oldItemId}.asset"
                             into itemPath
                             where AssetDatabase.LoadAssetAtPath<SettingsPageItemSO>(itemPath)
                             select itemPath)
                    {
                        AssetDatabase.DeleteAsset(itemPath);
                    }
                }
            }
        }

        private static string GetPageFolder(SettingsPageSO page) => $"{Root}/{page.uniqueId}";
        
        private static string GetCategoryFolder(SettingsPageSO page, SettingsPageCategorySO cat) => $"{GetPageFolder(page)}/{cat.uniqueId}";

        private static void DeleteFolderByPattern(string pattern)
        {
            var folders = AssetDatabase.GetAllAssetPaths()
                .Where(p => AssetDatabase.IsValidFolder(p) && p.Contains(pattern.Replace("*", "")));

            foreach (var folder in folders)
                AssetDatabase.DeleteAsset(folder);
        }
        
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var parts = path.Split('/');
            var current = parts[0];

            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        #endregion

        #region LOGIC

        private void AddPage()
        {
            var page = ScriptableObject.CreateInstance<SettingsPageSO>();
            page.uniqueId = GUID.Generate().ToString();
            page.pageName = "New Page";
            page.categories = new List<SettingsPageCategorySO>();

            _pages.Add(page);
            
            UpdatePageIndices();
            RefreshPageButtons();
        }
        
        private void UpdatePageIndices()
        {
            for (var i = 0; i < _pages.Count; i++)
                _pages[i].index = i;
        }
        
        private void GenerateCustomEnum()
        {
            var values = new List<string>();

            foreach (var item in _pages.SelectMany(page => page.categories.SelectMany(cat => cat.items)))
            {
                CollectCustomItems(item, values);
            }

            var sanitized = values
                .Select(SanitizeEnumName)
                .Where(v => !string.IsNullOrEmpty(v))
                .ToArray();

            EnumSynchronizer.Synchronize(
                enumPath: "Assets/Scripts/Utils/Enum/NamedCustomSetting.cs",
                enumNamespace: "Utils.Enum",
                enumName: "NamedCustomSetting",
                values: sanitized,
                logger: _logger
            );
        }
        
        private static void CollectCustomItems(SettingsPageItemSO item, List<string> result)
        {
            if (item.itemType == SettingsItemType.Custom)
            {
                result.Add(item.settingName);
            }

            if (item.itemType != SettingsItemType.Boolean ||
                !item.BooleanIsConditional ||
                item.ConditionalItems == null)
                return;
            foreach (var child in item.ConditionalItems)
            {
                CollectCustomItems(child, result);
            }
        }
        
        private static string SanitizeEnumName(string input)
        {
            return string.IsNullOrWhiteSpace(input) ? null : input.Replace(" ", "");

        }

        #endregion
    }
}