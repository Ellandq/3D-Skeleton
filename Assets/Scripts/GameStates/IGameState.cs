using System;

namespace GameStates
{
    public interface IGameState
    {
        NamedState Name { get; }
        GameStateStatus Status { get; }

        Action Initialize(Action onActivate);
        void Enter();
        void Activate();
        void Exit(Action newStateActivationHandle);
    }
}