using System.Collections.Generic;
using System.Linq;
using Editor.CommandCenter.Screens.SettingsEnforcement;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.SO.Settings.Screen;

namespace Editor.CommandCenter.Screens
{
    public class SettingsEnforcementScreen : ICommandCenterScreen
    {
        private const string SettingsPath =
            "Assets/ScriptableObjects/Settings/Pages";

        private readonly List<SettingsPageSO> _pages = new();

        private ICommandCenterLogger _logger;
        private ScrollView _scroll;

        public string ScreenName => "Settings Enforcement";
        
        private readonly Color _background = new(0.06f, 0.06f, 0.06f);
        private readonly Color _rowHover = new(0.15f, 0.15f, 0.15f);
        private readonly Color _textColor = new(0.85f, 0.85f, 0.85f);
        private readonly Color _lineColor = new(0.3f, 0.3f, 0.3f);

        public void Initialize(ICommandCenterLogger logger)
        {
            _logger = logger;
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
                    paddingTop = 12,
                    backgroundColor = _background
                }
            };

            var refreshButton = new Button(BuildTree)
            {
                text = "Refresh"
            };

            refreshButton.style.height = 35;
            refreshButton.style.marginBottom = 10;

            root.Add(refreshButton);

            _scroll = new ScrollView
            {
                style =
                {
                    flexGrow = 1,
                    backgroundColor = new Color(0.03f,0.03f,0.03f),
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 10
                }
            };

            var hierarchyPanel = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    backgroundColor =
                        new Color(0.04f,0.04f,0.04f),

                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6,

                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 8,
                    paddingBottom = 8
                }
            };

            hierarchyPanel.Add(_scroll);

            root.Add(hierarchyPanel);

            BuildTree();

            return root;
        }

        private void BuildTree()
        {
            _scroll.Clear();

            LoadPages();

            var resolver = new SettingsCoverageResolver();
            SettingsCoverageResolver.Refresh();

            foreach (var page in _pages.OrderBy(x => x.index))
            {
                _scroll.Add(
                    CreatePageNode(page, resolver)
                );
            }
        }

        private void LoadPages()
        {
            _pages.Clear();

            var guids = AssetDatabase.FindAssets(
                "t:SettingsPageSO",
                new[] { SettingsPath });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                var page = AssetDatabase.LoadAssetAtPath<SettingsPageSO>(path);

                if (page != null)
                    _pages.Add(page);
            }
        }


        private VisualElement CreatePageNode(
            SettingsPageSO page,
            SettingsCoverageResolver resolver)
        {
            var container = CreateNodeContainer();

            var header = CreateNodeHeader(
                page.pageName,
                resolver.GetPageStatus(page),
                page.pageName,
                SettingsItemType.Custom,
                "└──");

            container.Add(header);

            foreach (var category in page.categories)
            {
                container.Add(
                    CreateCategoryNode(
                        page,
                        category,
                        resolver)
                );
            }

            return container;
        }


        private VisualElement CreateCategoryNode(
            SettingsPageSO page,
            SettingsPageCategorySO category,
            SettingsCoverageResolver resolver)
        {
            var container = CreateIndentedContainer(20);

            container.Add(
                    CreateNodeHeader(
                        category.categoryName,
                        resolver.GetCategoryStatus(page, category),
                        $"{page.pageName}/{category.categoryName}",
                        SettingsItemType.Custom,
                        "    ├──")
            );


            foreach (var item in category.items)
            {
                container.Add(
                    CreateItemNode(
                        page,
                        category,
                        item,
                        resolver)
                );
            }

            return container;
        }


        private VisualElement CreateItemNode(
            SettingsPageSO page,
            SettingsPageCategorySO category,
            SettingsPageItemSO item,
            SettingsCoverageResolver resolver)
        {
            var container = CreateIndentedContainer(40);

            container.Add(
                    CreateNodeHeader(
                        item.settingName,
                        resolver.GetItemStatus(page,category,item),
                        $"{page.pageName}/{category.categoryName}/{item.settingName}",
                        item.itemType,
                        "        └──")
            );

            return container;
        }


        private VisualElement CreateNodeHeader(
            string text,
            CoverageStatus status,
            string key,
            SettingsItemType itemType,
            string prefix = "")
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    height = 32,
                    paddingLeft = 8,
                    paddingRight = 8
                }
            };


            var connector = new Label(prefix)
            {
                style =
                {
                    width = 45,
                    color = _lineColor,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 15
                }
            };


            var dot = new Label("●")
            {
                style =
                {
                    width = 20,
                    color = GetColor(status),
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };


            var label = new Label(text)
            {
                style =
                {
                    flexGrow = 1,
                    color = _textColor,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 15
                }
            };


            var button = new Button(() =>
            {
                SettingsEnforcerGenerator.Create(
                    text,
                    key,
                    itemType,
                    _logger);
            })
            {
                text = "Create enforcement"
            };


            button.style.display = DisplayStyle.None;
            button.style.width = 160;
            button.style.height = 24;


            button.style.backgroundColor =
                new Color(0.25f,0.25f,0.25f);

            button.style.color =
                Color.white;


            row.RegisterCallback<MouseEnterEvent>(_ =>
            {
                row.style.backgroundColor = _rowHover;
                button.style.display = DisplayStyle.Flex;
            });


            row.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                row.style.backgroundColor =
                    Color.clear;

                button.style.display =
                    DisplayStyle.None;
            });


            row.Add(connector);
            row.Add(dot);
            row.Add(label);
            row.Add(button);


            return row;
        }


        private static VisualElement CreateNodeContainer()
        {
            return new VisualElement();
        }


        private static VisualElement CreateIndentedContainer(int padding)
        {
            return new VisualElement
            {
                style =
                {
                    paddingLeft = padding
                }
            };
        }


        private static Color GetColor(CoverageStatus status)
        {
            return status switch
            {
                CoverageStatus.Green =>
                    new Color(0.3f, 0.8f, 0.3f),

                CoverageStatus.Yellow =>
                    new Color(0.9f, 0.7f, 0.2f),

                CoverageStatus.Red =>
                    new Color(0.9f, 0.3f, 0.3f),

                _ =>
                    Color.white
            };
        }
    }
}