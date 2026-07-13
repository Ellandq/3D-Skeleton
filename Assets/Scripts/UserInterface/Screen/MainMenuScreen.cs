using System;
using Managers;
using Model.Enum.Named;
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
        [SerializeField] private Button continueButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            settingsButton.onClick.AddListener(() => UIManager.ActivateComponent(NamedScreen.Settings));
            quitButton.onClick.AddListener(() => UIManager.ActivateComponent(NamedWindow.ExitConfirmation));
            newGameButton.onClick.AddListener((() =>
            {
                GameManager.ChangeState(NamedState.Gameplay);
                SaveManager.SetSelectedSave("");
                GameManager.PushState(NamedState.GameLoad);
            }));
            continueButton.onClick.AddListener((() =>
            {
                GameManager.ChangeState(NamedState.Gameplay);
                SaveManager.SetSelectedSave(SaveManager.GetMostRecentSaveName());
                GameManager.PushState(NamedState.GameLoad);
            }));
        }

        private void OnEnable()
        {
            continueButton.enabled = !string.IsNullOrEmpty(SaveManager.GetMostRecentSaveName());
        }
    }
}