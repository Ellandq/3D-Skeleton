using Cysharp.Threading.Tasks;
using Managers;
using UserInterface.Screen;
using Utils.Enum;

namespace GameStates
{
    public class NewGameState : IGameState
    {
        public NamedState Name => NamedState.NewGame;
        public void Enter(NamedState? previousState = null)
        {
            _ = LoadNewGameAsync();
        }
        
        private static async UniTask LoadNewGameAsync()
        {
            await GameManager.LoadHandle.LoadGame(
                NamedScene.Gameplay,
                () =>
                {
                    UIManager.DeactivateComponent(NamedScreen.Loading, false,
                        () => GameManager.ChangeState(NamedState.Gameplay));
                    UIManager.SetOnEmptyStackExitCallback(() =>
                    {
                        UIManager.ActivateComponent(NamedScreen.Settings);
                        GameManager.PushState(NamedState.PauseMenu);
                    });
                },
                null
            );
        }

        public void Pause() { }

        public void Resume() { }

        public void Exit() { }
    }
}