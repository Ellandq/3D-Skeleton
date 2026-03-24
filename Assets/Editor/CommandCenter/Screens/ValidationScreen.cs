using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Editor.CommandCenter.Screens.Modules.Validation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.CommandCenter.Screens
{
    public class ValidationScreen : ICommandCenterScreen
    {
        private readonly List<IEditorValidationModule> _modules = new();
        private readonly Dictionary<IEditorValidationModule, Label> _statusIndicators = new();

        private ICommandCenterLogger _logger;
        private ScrollView _moduleScroll;

        public string ScreenName => "Validation";
        
        private const string FoldoutPrefsKey = "CommandCenter_Foldout_";

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
                    paddingTop = 20,
                    paddingBottom = 10,
                    paddingLeft = 20,
                    paddingRight = 20
                }
            };

            var topBar = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.Center,
                    marginBottom = 16
                }
            };

            var validateAllButton = new Button(ValidateAll)
            {
                text = "Validate All"
            };
            StyleButton(validateAllButton);

            var enforceAllButton = new Button(EnforceAll)
            {
                text = "Enforce All"
            };
            StyleButton(enforceAllButton);

            topBar.Add(validateAllButton);
            topBar.Add(enforceAllButton);

            root.Add(topBar);

            _moduleScroll = new ScrollView
            {
                style =
                {
                    flexGrow = 1,
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 8
                }
            };
            root.Add(_moduleScroll);

            DiscoverModules();
            RunAutoValidation();

            return root;

            void StyleButton(Button btn)
            {
                btn.style.height = 40;
                btn.style.width = 160;
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                btn.style.fontSize = 14;
                btn.style.backgroundColor = new Color(0.42f, 0.42f, 0.42f);
                btn.style.color = Color.white;
                btn.style.unityTextAlign = TextAnchor.MiddleCenter;
                btn.style.marginLeft = 6;
                btn.style.marginRight = 6;
                btn.style.paddingLeft = 8;
                btn.style.paddingRight = 8;
                btn.style.borderTopLeftRadius = 6;
                btn.style.borderTopRightRadius = 6;
                btn.style.borderBottomLeftRadius = 6;
                btn.style.borderBottomRightRadius = 6;
                btn.style.unityBackgroundImageTintColor = Color.white;
            }
        }

        private void DiscoverModules()
        {
            _modules.Clear();
            _moduleScroll.Clear();

            var moduleTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t =>
                    typeof(IEditorValidationModule).IsAssignableFrom(t) &&
                    !t.IsInterface &&
                    !t.IsAbstract);

            foreach (var type in moduleTypes)
            {
                var module = (IEditorValidationModule)Activator.CreateInstance(type);
                module.Initialize(_logger);
                _modules.Add(module);

                _moduleScroll.Add(CreateModuleUI(module));
            }
        }
        
        private void RunAutoValidation()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            foreach (var module in _modules)
                module.Validate();

            RefreshStatuses();
        }
        
        private VisualElement CreateModuleUI(IEditorValidationModule validationModule)
        {
            var card = new VisualElement
            {
                style =
                {
                    marginBottom = 12,
                    backgroundColor = new Color(0.24f, 0.24f, 0.24f),
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = new Color(0.1f, 0.1f, 0.1f),
                    borderBottomColor = new Color(0.1f, 0.1f, 0.1f),
                    borderLeftColor = new Color(0.1f, 0.1f, 0.1f),
                    borderRightColor = new Color(0.1f, 0.1f, 0.1f)
                }
            };

            var isOpen = EditorPrefs.GetBool(FoldoutPrefsKey + validationModule.ModuleName, true);

            var header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 6,
                    paddingBottom = 6,
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f),
                    borderBottomWidth = 1,
                    borderBottomColor = new Color(0.1f, 0.1f, 0.1f)
                }
            };

            var label = new Label(validationModule.ModuleName)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    flexGrow = 1
                }
            };

            var statusDot = new Label("●")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };

            _statusIndicators[validationModule] = statusDot;
            UpdateIndicator();

            header.Add(label);
            header.Add(statusDot);

            var contentContainer = new VisualElement
            {
                style =
                {
                    display = isOpen ? DisplayStyle.Flex : DisplayStyle.None,
                    paddingLeft = 12,
                    paddingRight = 12,
                    paddingTop = 10,
                    paddingBottom = 12,
                    marginTop = 4
                }
            };

            var scroll = new ScrollView
            {
                style =
                {
                    maxHeight = 300
                }
            };
            scroll.Add(validationModule.CreateContent());

            contentContainer.Add(scroll);

            var buttonRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginTop = 6
                }
            };

            var validate = new Button(() =>
            {
                validationModule.Validate();
                UpdateIndicator();
            }) { text = "Validate" };

            var enforce = new Button(() =>
            {
                validationModule.Enforce();
                UpdateIndicator();
            }) { text = "Enforce" };

            validate.style.marginRight = 4;

            buttonRow.Add(validate);
            buttonRow.Add(enforce);
            contentContainer.Add(buttonRow);

            header.RegisterCallback<ClickEvent>(_ =>
            {
                isOpen = !isOpen;
                contentContainer.style.display = isOpen ? DisplayStyle.Flex : DisplayStyle.None;
                EditorPrefs.SetBool(FoldoutPrefsKey + validationModule.ModuleName, isOpen);
            });

            card.Add(header);
            card.Add(contentContainer);

            return card;

            void UpdateIndicator()
            {
                statusDot.style.color = GetStatusColor(validationModule.Status);
            }
        }

        private void ValidateAll()
        {
            foreach (var module in _modules)
                module.Validate();

            RefreshStatuses();
        }

        private void EnforceAll()
        {
            foreach (var module in _modules)
                module.Enforce();

            RefreshStatuses();
        }
        
        private void RefreshStatuses()
        {
            foreach (var pair in _statusIndicators)
            {
                pair.Value.style.color = GetStatusColor(pair.Key.Status);
            }
        }

        private static Color GetStatusColor(ModuleStatus status)
        {
            return status switch
            {
                ModuleStatus.Valid => new Color(0.3f, 0.8f, 0.3f),
                ModuleStatus.Warning => new Color(0.9f, 0.7f, 0.2f),
                ModuleStatus.Error => new Color(0.9f, 0.3f, 0.3f),
                _ => new Color(0.5f, 0.5f, 0.5f)
            };
        }
    }
}