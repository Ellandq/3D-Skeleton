using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.CommandCenter
{
    public class CommandCenterWindow : EditorWindow, ICommandCenterLogger
    {
        private readonly List<ICommandCenterScreen> _screens = new();
        private ICommandCenterScreen _activeScreen;
        
        private static readonly List<(string message, Color color)> LOGHistory = new();

        private VisualElement _screenContainer;
        private VisualElement _screenButtonBar;
        
        private ScrollView _moduleScroll;
        private ScrollView _consoleScroll;

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
            CreateScreenContainer();
            CreateConsole();

            foreach (var entry in LOGHistory)
                AppendLogToUI(entry.message, entry.color);

            DiscoverScreens();
        }

        #region Header

        private void CreateHeader()
        {
            var header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    backgroundColor = new Color(0.22f, 0.22f, 0.22f),
                    borderBottomWidth = 1,
                    borderBottomColor = new Color(0.1f, 0.1f, 0.1f)
                }
            };

            var titleRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 6,
                    paddingBottom = 4
                }
            };

            var label = new Label("COMMAND CENTER")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    flexGrow = 1,
                    fontSize = 16
                }
            };

            titleRow.Add(label);

            _screenButtonBar = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                    paddingLeft = 6,
                    paddingBottom = 6,
                    marginTop = 4
                }
            };

            header.Add(titleRow);
            header.Add(_screenButtonBar);

            rootVisualElement.Add(header);
        }
        
        #endregion
        
        private void CreateScreenContainer()
        {
            _screenContainer = new VisualElement
            {
                style = { flexGrow = 1 }
            };

            rootVisualElement.Add(_screenContainer);
        }
        
        private void DiscoverScreens()
        {
            _screens.Clear();
            _screenButtonBar.Clear();

            var screenTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t =>
                    typeof(ICommandCenterScreen).IsAssignableFrom(t) &&
                    !t.IsInterface &&
                    !t.IsAbstract);

            foreach (var type in screenTypes)
            {
                var screen = (ICommandCenterScreen)Activator.CreateInstance(type);
                screen.Initialize(this);

                _screens.Add(screen);

                var button = new Button(() => ShowScreen(screen))
                {
                    text = screen.ScreenName,
                    style =
                    {
                        marginRight = 6,
                        marginBottom = 6,
                        paddingLeft = 12,
                        paddingRight = 12,
                        paddingTop = 6,
                        paddingBottom = 6,
                        fontSize = 14,
                        unityTextAlign = TextAnchor.MiddleCenter,
                        backgroundColor = new Color(0.33f, 0.33f, 0.33f),
                        color = Color.white,
                        unityFontStyleAndWeight = FontStyle.Bold,
                    }
                };

                button.RegisterCallback<MouseEnterEvent>(_ => button.style.backgroundColor = new Color(0.35f, 0.35f, 0.35f));
                button.RegisterCallback<MouseLeaveEvent>(_ => button.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f));

                _screenButtonBar.Add(button);
            }

            if (_screens.Count > 0)
                ShowScreen(_screens[0]);
        }
        
        private void ShowScreen(ICommandCenterScreen screen)
        {
            _screenContainer.Clear();

            var content = screen.CreateContent();
            _screenContainer.Add(content);

            _activeScreen = screen;
        }

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
            LOGHistory.Add((message, color));
            AppendLogToUI(message, color);
        }
        
        private void AppendLogToUI(string message, Color color)
        {
            var label = new Label(message)
            {
                style = { color = color }
            };

            _consoleScroll.Add(label);

            label.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                _consoleScroll.verticalScroller.value =
                    _consoleScroll.verticalScroller.highValue;
            });
        }

        #endregion
    }
}