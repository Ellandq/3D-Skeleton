using Cysharp.Threading.Tasks;
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
        
        public void Enter(NamedState? previousState = null)
        {
            _ = LoadMainMenuAsync();
        }

        private async UniTask LoadMainMenuAsync()
        {
            await GameManager.LoadHandle.LoadGame(
                NamedScene.MainMenu,
                () =>
                {
                    UIManager.DeactivateComponent(NamedScreen.Loading, false, Resume);
                    UIManager.ActivateComponent(NamedScreen.MainMenu);
                    UIManager.SetOnEmptyStackExitCallback(() => UIManager.ActivateComponent(NamedWindow.ExitConfirmation));
                },
                null
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