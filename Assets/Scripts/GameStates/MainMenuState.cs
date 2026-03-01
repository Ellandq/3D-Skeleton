using System.Threading.Tasks;
using Managers;
using UnityEngine;
using UserInterface.Screen;
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
                () => UIManager.Instance.DeactivateComponent(NamedScreen.Loading, false, Resume)
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