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
                    UIManager.DeactivateComponent(NamedScreen.Loading, false, Resume);
                    UIManager.ActivateComponent(NamedScreen.MainMenu);
                    UIManager.SetOnEmptyStackExitCallback(() => UIManager.ActivateComponent(NamedWindow.ExitConfirmation));
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