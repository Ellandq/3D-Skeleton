using Model.Enum.Named;

namespace Managers.GameStates
{
    public class PauseMenuState : IGameState
    {
        public NamedState Name => NamedState.PauseMenu;
        
        public void Enter(NamedState? previousState = null)
        {
            
        }

        public void Pause() { }

        public void Resume() { }

        public void Exit() { }
    }
}