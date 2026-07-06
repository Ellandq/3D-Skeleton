using Model.Enum;
using Model.Enum.Named;
using UnityEngine;

namespace Managers.GameStates
{
    public class GameplayState : IGameState
    {
        public NamedState Name => NamedState.Gameplay;
        
        public void Enter(NamedState? previousState = null)
        {
            
        }

        public void Pause()
        {
            Time.timeScale = 0f;
        }

        public void Resume()
        {
            Time.timeScale = 1f;
        }

        public void Exit() { }
    }
}