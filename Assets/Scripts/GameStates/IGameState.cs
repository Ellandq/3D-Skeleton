using System;
using Utils.Enum;

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