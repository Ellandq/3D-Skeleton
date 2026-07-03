using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Editor.CommandCenter.Screens.Modules.SceneAssets;
using UnityEditor.SceneManagement;

namespace Editor.CommandCenter.Screens
{
    public class SceneAssetManagerScreen : ICommandCenterScreen
    {
        public string ScreenName => "Props";

        private ICommandCenterLogger _logger;

        private SceneAssetManagerService _service;

        private PopupField<string> _sceneDropdown;

        private TextField _searchField;

        private ListView _assetTypeList;
        private ListView _assetList;

        private VisualElement _inspector;

        private readonly List<AssetTypeViewModel> _filteredTypes = new();
        private readonly List<ScenePropViewModel> _displayedAssets = new();

        private SceneComparison _comparison;

        private AssetTypeViewModel _selectedType;
        private ScenePropViewModel _selectedAsset;

        private static readonly Color SavedColor = new(0.35f, 0.65f, 1f);
        private static readonly Color SceneOnlyColor = new(0.65f, 0.65f, 0.65f);
        private static readonly Color ModifiedColor = new(1f, 0.85f, 0.2f);

        public void Initialize(ICommandCenterLogger logger)
        {
            _logger = logger;
            _service = new SceneAssetManagerService(logger);
            
            
            EditorSceneManager.sceneOpened += OnSceneChanged;
            EditorSceneManager.sceneClosed += OnSceneClosed;
            EditorSceneManager.newSceneCreated += OnNewSceneCreated;
        }

        public VisualElement CreateContent()
        {
            var root = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Column,
                    paddingTop = 10,
                    paddingBottom = 10,
                    paddingLeft = 10,
                    paddingRight = 10
                }
            };

            root.Add(CreateToolbar());

            var split = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                    marginTop = 8
                }
            };

            split.Add(CreateLeftPanel());
            split.Add(CreateRightPanel());

            root.Add(split);

            _inspector = CreateInspector();
            root.Add(_inspector);

            RefreshSceneList();

            return root;
        }

        private VisualElement CreateToolbar()
        {
            var toolbar = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    height = 40,
                    marginBottom = 6
                }
            };

            var refreshButton = new Button(OnRefreshPressed)
            {
                text = "Refresh"
            };

            StyleButton(refreshButton);
            refreshButton.style.width = 90;

            toolbar.Add(refreshButton);

            var scenes = GetLoadedSceneNames();

            _sceneDropdown = new PopupField<string>(
                scenes,
                scenes.Count > 0 ? 0 : -1)
            {
                style =
                {
                    width = 350,
                    height = 28,
                    marginLeft = 8,
                    marginRight = 8
                }
            };

            _sceneDropdown.style.fontSize = 14;
            _sceneDropdown.labelElement.style.fontSize = 14;
            
            _sceneDropdown.RegisterValueChangedCallback(_ =>
            {
                ClearAssetDisplay();
            });

            toolbar.Add(_sceneDropdown);

            toolbar.Add(new VisualElement
            {
                style =
                {
                    flexGrow = 1
                }
            });

            var spawnButton = new Button(OnSpawnPressed)
            {
                text = "Spawn Assets"
            };

            StyleButton(spawnButton);

            var saveButton = new Button(OnSavePressed)
            {
                text = "Save"
            };

            StyleButton(saveButton);

            var destroyButton = new Button(OnDestroyPressed)
            {
                text = "Deload"
            };

            StyleButton(destroyButton);

            toolbar.Add(spawnButton);
            toolbar.Add(saveButton);
            toolbar.Add(destroyButton);

            return toolbar;
        }
        
        private void ClearAssetDisplay()
        {
            _comparison = null;

            _filteredTypes.Clear();
            _displayedAssets.Clear();

            _selectedType = null;
            _selectedAsset = null;

            _assetTypeList.itemsSource = _filteredTypes;
            _assetTypeList.Rebuild();

            _assetList.itemsSource = _displayedAssets;
            _assetList.Rebuild();

            RefreshInspector();
        }

        private VisualElement CreateLeftPanel()
        {
            var panel = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    marginRight = 4,
                    flexDirection = FlexDirection.Column
                }
            };

            var searchRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = 28,
                    marginBottom = 6
                }
            };

            _searchField = new TextField
            {
                style =
                {
                    flexGrow = 1
                }
            };

            _searchField.RegisterValueChangedCallback(_ => ApplySearch());

            var resetButton = new Button(() =>
            {
                _searchField.value = "";
            })
            {
                text = "Reset",
                style =
                {
                    width = 60
                }
            };

            searchRow.Add(_searchField);
            searchRow.Add(resetButton);

            panel.Add(searchRow);

            _assetTypeList = CreateAssetTypeList();

            panel.Add(_assetTypeList);

            return panel;
        }

        private VisualElement CreateRightPanel()
        {
            var panel = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    marginLeft = 4,
                    flexDirection = FlexDirection.Column
                }
            };

            _assetList = CreateAssetList();

            panel.Add(_assetList);

            return panel;
        }

        private static VisualElement CreateInspector()
        {
            var container = new VisualElement
            {
                style =
                {
                    marginTop = 8,
                    height = 220,
                    backgroundColor = new Color(.19f,.19f,.19f),
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = new Color(.08f,.08f,.08f),
                    borderBottomColor = new Color(.08f,.08f,.08f),
                    borderLeftColor = new Color(.08f,.08f,.08f),
                    borderRightColor = new Color(.08f,.08f,.08f),
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 8
                }
            };

            var title = new Label("Inspector")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14,
                    marginBottom = 6
                }
            };

            container.Add(title);

            container.Add(new Label("Select an asset to inspect."));

            return container;
        }

        private static void StyleButton(Button button)
        {
            button.style.height = 28;
            button.style.marginLeft = 4;
            button.style.marginRight = 4;
            button.style.paddingLeft = 10;
            button.style.paddingRight = 10;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.backgroundColor = new Color(.34f,.34f,.34f);
            button.style.color = Color.white;
        }

        private static List<string> GetLoadedSceneNames()
        {
            var result = new List<string>();

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);

                if (scene.IsValid() && scene.isLoaded)
                    result.Add(scene.name);
            }

            return result;
        }

        private void RefreshSceneList()
        {
            var previous = _sceneDropdown?.value;

            var scenes = GetLoadedSceneNames();

            if (scenes.Count == 0)
                scenes.Add("No Loaded Scenes");

            if (_sceneDropdown == null) return;
            _sceneDropdown.choices = scenes;

            var index = scenes.IndexOf(previous);

            _sceneDropdown.index = index >= 0 ? index : 0;
        }

        private ListView CreateAssetTypeList()
        {
            var list = new ListView
            {
                selectionType = SelectionType.Single,
                style =
                {
                    flexGrow = 1,
                    backgroundColor = new Color(.15f,.15f,.15f)
                },
                makeItem = () =>
                {
                    var label = new Label();

                    StyleListLabel(label);

                    return label;
                },
                bindItem = (element, index) =>
                {
                    var label = element as Label;

                    var item = _filteredTypes[index];

                    if (label == null) return;
                    label.text = item.displayName;

                    label.style.color =
                        item.isSaved
                            ? SavedColor
                            : SceneOnlyColor;
                }
            };

            list.selectionChanged += selection =>
            {
                var selected = selection.FirstOrDefault();

                if (selected is not AssetTypeViewModel type) return;
                _selectedType = type;
                RefreshAssetList();
            };

            return list;
        }
        
        private ListView CreateAssetList()
        {
            var list = new ListView
            {
                selectionType = SelectionType.Single,
                style =
                {
                    flexGrow = 1,
                    backgroundColor = new Color(.15f,.15f,.15f)
                },
                makeItem = () => new Label
                {
                    style =
                    {
                        paddingLeft = 8,
                        paddingTop = 4,
                        paddingBottom = 4
                    }
                },
                bindItem = (element, index) =>
                {
                    var label = element as Label;

                    var asset = _displayedAssets[index];

                    label.text = asset.displayName;

                    if (asset.isModified)
                    {
                        label.style.color = ModifiedColor;
                    }
                    else
                    {
                        label.style.color =
                            asset.isSaved
                                ? SavedColor
                                : SceneOnlyColor;
                    }
                }
            };


            list.selectionChanged += selection =>
            {
                var selected = selection.FirstOrDefault();

                if (selected is not ScenePropViewModel prop) return;
                _selectedAsset = prop;
                RefreshInspector();
            };
            
            return list;
        }

        private void ApplySearch()
        {
            if (_comparison == null)
                return;
            
            var search = _searchField.value;
            
            if (string.IsNullOrWhiteSpace(search))
            {
                _filteredTypes.Clear();

                _filteredTypes.AddRange(
                    _comparison.assetTypes);
            }
            else
            {
                _filteredTypes.Clear();

                _filteredTypes.AddRange(
                    _comparison.assetTypes
                        .Where(x =>
                            x.displayName
                             .Contains(
                                 search,
                                 System.StringComparison
                                 .OrdinalIgnoreCase)));
            }
            
            _assetTypeList.itemsSource = _filteredTypes;
            _assetTypeList.Rebuild();
        }
        
        private void RefreshAssetList()
        {
            _displayedAssets.Clear();
            
            if (_selectedType != null)
            {
                _displayedAssets.AddRange(
                    _selectedType.assets);
            }

            _assetList.itemsSource = _displayedAssets;
            _assetList.Rebuild();
            _selectedAsset = null;

            RefreshInspector();
        }

        private void RefreshInspector()
        {
            if (_inspector == null)
                return;
            
            _inspector.Clear();
            
            var title = new Label("Inspector")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14,
                    marginBottom = 6
                }
            };

            _inspector.Add(title);
            
            if (_selectedAsset == null)
            {
                _inspector.Add(
                    new Label("Select an asset."));
                return;
            }
            
            AddInspectorValue(
                "Name",
                _selectedAsset.displayName);
            
            AddInspectorValue(
                "Address",
                _selectedAsset.address);
            
            AddInspectorValue(
                "Position",
                _selectedAsset.position.ToString());

            AddInspectorValue(
                "Rotation",
                _selectedAsset.rotation.ToString());

            AddInspectorValue(
                "Scale",
                _selectedAsset.scale.ToString());
            
            if (!_selectedAsset.isDynamic)
                return;

            AddInspectorValue(
                "Mass",
                _selectedAsset.mass.ToString(CultureInfo.InvariantCulture));

            AddInspectorValue(
                "Interpolation",
                _selectedAsset.interpolation.ToString());

            AddInspectorValue(
                "Collision Mode",
                _selectedAsset.collisionDetectionMode.ToString());

            AddInspectorValue(
                "Velocity",
                _selectedAsset.velocity.ToString());

            AddInspectorValue(
                "Angular Velocity",
                _selectedAsset.angularVelocity.ToString());

            AddInspectorValue(
                "Gravity",
                _selectedAsset.usesGravity.ToString());

            AddInspectorValue(
                "Kinematic",
                _selectedAsset.isKinematic.ToString());
            
            AddInspectorValue(
                "Save Data",
                string.IsNullOrEmpty(_selectedAsset.saveData)
                    ? "<None>"
                    : _selectedAsset.saveData);
        }

        private void AddInspectorValue(string name, string value)
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginBottom = 3
                }
            };
            
            var key = new Label(name)
            {
                style =
                {
                    width = 130,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            
            var val = new Label(value);

            row.Add(key);
            row.Add(val);
            
            _inspector.Add(row);
        }

        private void OnRefreshPressed()
        {
            if (_sceneDropdown.value == "No Loaded Scenes")
                return;
            
            _comparison =
                _service.ScanScene(
                    _sceneDropdown.value);
            
            ApplySearch();

            _logger.Log(
                $"Scanned {_sceneDropdown.value}");
        }

        private void OnSpawnPressed()
        {
            if (_comparison == null)
                return;

            _service.SpawnMissing(
                _comparison);
        }

        private void OnSavePressed()
        {
            if (_comparison == null)
                return;

            _service.Save(
                _comparison);
            
            _comparison =
                _service.ScanScene(
                    _sceneDropdown.value);

            ApplySearch();

            if (_selectedType == null) return;
            var updated =
                _comparison.assetTypes
                    .FirstOrDefault(
                        x =>
                            x.address == _selectedType.address);


            _selectedType = updated;

            RefreshAssetList();
        }
        
        private void OnSceneChanged(Scene scene, OpenSceneMode mode)
        {
            RefreshSceneList();
        }

        private void OnSceneClosed(Scene scene)
        {
            RefreshSceneList();
        }

        private void OnNewSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode)
        {
            RefreshSceneList();
        }
        
        private static void StyleListLabel(Label label)
        {
            label.style.fontSize = 14;
            label.style.unityFontStyleAndWeight =
                FontStyle.Bold;

            label.style.paddingLeft = 8;
            label.style.paddingTop = 5;
            label.style.paddingBottom = 5;
        }

        private void OnDestroyPressed()
        {
            _service.DestroySceneProps();
        }
    }
}