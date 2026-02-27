using System;

namespace GameStates
{
    public interface IGameState
    {
        NamedState Name { get; }
        
        void Enter();
        void Pause();
        void Resume();
        void Exit();
    }
}