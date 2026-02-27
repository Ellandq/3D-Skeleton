using Utils.Enum;

namespace GameStates
{
    public class MainMenuState : IGameState
    {
        public NamedState Name => NamedState.MainMenu;
        
        public void Enter()
        {
            
        }

        public void Pause() { }

        public void Resume() { }

        public void Exit()
        {
            
        }
    }
}