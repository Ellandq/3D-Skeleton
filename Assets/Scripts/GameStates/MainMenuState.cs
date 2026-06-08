using System.Threading.Tasks;
using Managers;
using UnityEngine;
using UserInterface.Screen;
using UserInterface.Windows;
using Utils.Enum;

namespace GameStates
{
    public class MainMenuState : IGameState
    {
        public NamedState Name => NamedState.MainMenu;
        
        public void Enter()
        {
            _ = LoadMainMenuAsync();
        }

        private async Task LoadMainMenuAsync()
        {
            await GameManager.LoadHandle.LoadGame(
                NamedScene.MainMenu,
                () =>
                {
                    var uiManager = UIManager.Instance;
                    uiManager.DeactivateComponent(NamedScreen.Loading, false, Resume);
                    uiManager.ActivateComponent(NamedScreen.MainMenu);
                    uiManager.SetOnEmptyStackExitCallback(() => uiManager.ActivateComponent(NamedWindow.ExitConfirmation));
                }
            );
        }

        public void Pause() { }

        public void Resume()
        {
            Time.timeScale = 1f;
        }

        public void Exit()
        {
            
        }
    }
}