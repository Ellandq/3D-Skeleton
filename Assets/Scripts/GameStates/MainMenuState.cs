namespace GameStates
{
    public class MainMenuState : IGameState
    {
        public NamedState Name => NamedState.MainMenu;
        
        public void Enter()
        {
            /* TODO:
              - Open loading screen
              - 

             */
        }

        public void Pause() { }

        public void Resume() { }

        public void Exit()
        {
            /* TODO:
             - Deload 
             
            */
        }
    }
}