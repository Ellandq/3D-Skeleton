using System;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.Windows;

namespace UserInterface.Screen
{
    public class MainMenuScreen : ScreenBase
    {
        public override NamedScreen Name => NamedScreen.MainMenu;
        public override UIPriority Priority => UIPriority.Low;

        [Header("Components")] 
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            quitButton.onClick.AddListener(() => UIManager.Instance.ActivateComponent(NamedWindow.ExitConfirmation));
        }
    }
}