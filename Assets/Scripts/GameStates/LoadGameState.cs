using Utils.Enum;

namespace GameStates
{
    public class LoadGameState : IGameState
    {
        public NamedState Name => NamedState.LoadGame;
        
        public void Enter(NamedState? previousState = null)
        {
            if (previousState == NamedState.Gameplay)
            {
                
            }
        }

        public void Pause()
        {
            throw new System.NotImplementedException();
        }

        public void Resume()
        {
            throw new System.NotImplementedException();
        }

        public void Exit()
        {
            throw new System.NotImplementedException();
        }
    }
}