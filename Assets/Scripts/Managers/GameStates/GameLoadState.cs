using Cysharp.Threading.Tasks;
using Model.Enum.Named;
using UnityEngine;
using UserInterface.Screen;
using Utils.Enum;

namespace Managers.GameStates
{
    public class GameLoadState : IGameState
    {
        public NamedState Name => NamedState.GameLoad;

        public void Enter(NamedState? previousState = null)
        {
            _ = LoadNewGameAsync();
        }
        
        private static async UniTask LoadNewGameAsync()
        {
            Time.timeScale = 0f;
            await GameManager.LoadHandle.LoadGame(
                NamedScene.Gameplay,
                () =>
                {
                    UIManager.DeactivateComponent(NamedScreen.Loading, false,
                        () => GameManager.PopState());
                    UIManager.SetOnEmptyStackExitCallback(() =>
                    {
                        UIManager.ActivateComponent(NamedScreen.Settings);
                        GameManager.PushState(NamedState.PauseMenu);
                    });
                }
            );
        }

        public void Pause() { }

        public void Resume() { }

        public void Exit() { }
    }
}