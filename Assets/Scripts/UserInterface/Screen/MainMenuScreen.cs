using Managers;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.Windows;
using Utils.Enum;

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
            newGameButton.onClick.AddListener((() => GameManager.Instance.ChangeState(NamedState.Gameplay)));
        }
    }
}