using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Editor.CommandCenter.Modules;

namespace Editor.CommandCenter
{
    public class CommandCenterWindow : EditorWindow, ICommandCenterLogger
    {
        private readonly List<IEditorModule> _modules = new();
        private ScrollView _moduleScroll;
        private ScrollView _consoleScroll;

        private const string FoldoutPrefsKey = "CommandCenter_Foldout_";

        [MenuItem("Tools/Command Center")]
        public static void OpenWindow()
        {
            var window = GetWindow<CommandCenterWindow>();
            window.titleContent = new GUIContent("Command Center");
            window.minSize = new Vector2(700, 500);
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);

            CreateHeader();
            CreateModuleArea();
            CreateConsole();
            DiscoverModules();
        }

        #region Header

        private void CreateHeader()
        {
            var header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 6,
                    paddingBottom = 6,
                    backgroundColor = new Color(0.22f, 0.22f, 0.22f),
                    borderBottomWidth = 1,
                    borderBottomColor = new Color(0.1f, 0.1f, 0.1f)
                }
            };

            var label = new Label("COMMAND CENTER")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    flexGrow = 1,
                    unityTextAlign = TextAnchor.MiddleLeft
                }
            };

            var validateAll = new Button(() =>
            {
                foreach (var m in _modules)
                    m.Validate();
                RefreshStatuses();
            })
            { text = "Validate All" };

            var enforceAll = new Button(() =>
            {
                foreach (var m in _modules)
                    m.Enforce();
                RefreshStatuses();
            })
            { text = "Enforce All" };

            validateAll.style.marginRight = 6;

            header.Add(label);
            header.Add(validateAll);
            header.Add(enforceAll);

            rootVisualElement.Add(header);
        }

        #endregion

        #region Module Area

        private void CreateModuleArea()
        {
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

            rootVisualElement.Add(_moduleScroll);
        }

        private void DiscoverModules()
        {
            _modules.Clear();
            _moduleScroll.Clear();

            var moduleTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t =>
                    typeof(IEditorModule).IsAssignableFrom(t) &&
                    !t.IsInterface &&
                    !t.IsAbstract);

            foreach (var type in moduleTypes)
            {
                var module = (IEditorModule)Activator.CreateInstance(type);
                module.Initialize(this);
                _modules.Add(module);

                _moduleScroll.Add(CreateModuleUI(module));
            }
        }

        private VisualElement CreateModuleUI(IEditorModule module)
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

            var isOpen = EditorPrefs.GetBool(FoldoutPrefsKey + module.ModuleName, true);

            // HEADER
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

            var label = new Label(module.ModuleName)
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

            void UpdateIndicator()
            {
                statusDot.style.color = GetStatusColor(module.Status);
            }

            UpdateIndicator();

            header.Add(label);
            header.Add(statusDot);

            // CONTENT
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
            scroll.Add(module.CreateContent());

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
                module.Validate();
                UpdateIndicator();
            }) { text = "Validate" };

            var enforce = new Button(() =>
            {
                module.Enforce();
                UpdateIndicator();
            }) { text = "Enforce" };

            validate.style.marginRight = 4;

            buttonRow.Add(validate);
            buttonRow.Add(enforce);
            contentContainer.Add(buttonRow);

            // TOGGLE BEHAVIOR
            header.RegisterCallback<ClickEvent>(_ =>
            {
                isOpen = !isOpen;
                contentContainer.style.display = isOpen ? DisplayStyle.Flex : DisplayStyle.None;
                EditorPrefs.SetBool(FoldoutPrefsKey + module.ModuleName, isOpen);
            });

            card.Add(header);
            card.Add(contentContainer);

            return card;
        }
                
        private void RefreshStatuses()
        {
            Repaint();
        }

        private Color GetStatusColor(ModuleStatus status)
        {
            return status switch
            {
                ModuleStatus.Valid => new Color(0.3f, 0.8f, 0.3f),
                ModuleStatus.Warning => new Color(0.9f, 0.7f, 0.2f),
                ModuleStatus.Error => new Color(0.9f, 0.3f, 0.3f),
                _ => new Color(0.5f, 0.5f, 0.5f)
            };
        }

        #endregion

        #region Console

        private void CreateConsole()
        {
            var consoleContainer = new VisualElement
            {
                style =
                {
                    height = 160,
                    marginTop = 6,
                    backgroundColor = new Color(0.12f, 0.12f, 0.12f),
                    borderTopWidth = 1,
                    borderTopColor = new Color(0.05f, 0.05f, 0.05f)
                }
            };

            var label = new Label("Console")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginLeft = 8,
                    marginTop = 4
                }
            };

            _consoleScroll = new ScrollView
            {
                style =
                {
                    flexGrow = 1,
                    paddingLeft = 8,
                    paddingRight = 8
                }
            };

            consoleContainer.Add(label);
            consoleContainer.Add(_consoleScroll);

            rootVisualElement.Add(consoleContainer);
        }

        public void Log(string message)
        {
            AddLog(message, new Color(0.8f, 0.8f, 0.8f));
        }

        public void LogWarning(string message)
        {
            AddLog(message, new Color(0.9f, 0.7f, 0.2f));
        }

        public void LogError(string message)
        {
            AddLog(message, new Color(0.9f, 0.3f, 0.3f));
        }

        private void AddLog(string message, Color color)
        {
            var label = new Label(message)
            {
                style =
                {
                    color = color
                }
            };
            _consoleScroll.Add(label);
            _consoleScroll.scrollOffset = new Vector2(0, float.MaxValue);
        }

        #endregion
    }
}