using System.Threading.Tasks;
using Managers;
using UnityEngine;
using UserInterface.Screen;
using Utils.Enum;

namespace GameStates
{
    public class GameplayState : IGameState
    {
        public NamedState Name => NamedState.Gameplay;
        
        public void Enter()
        {
            _ = LoadGameplayAsync();
        }
        
        private async Task LoadGameplayAsync()
        {
            Time.timeScale = 0f;
            await GameManager.LoadHandle.LoadGame(
                NamedScene.Gameplay,
                () =>
                {
                    UIManager.DeactivateComponent(NamedScreen.Loading, false, Resume);
                    UIManager.SetOnEmptyStackExitCallback(() => UIManager.ActivateComponent(NamedScreen.Settings));
                }
            );
        }

        public void Pause() { }

        public void Resume()
        {
            Time.timeScale = 1f;
        }

        public void Exit() { }
    }
}