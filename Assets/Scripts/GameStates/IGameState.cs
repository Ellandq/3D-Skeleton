using Utils.Enum;

namespace GameStates
{
    public interface IGameState
    {
        NamedState Name { get; }
        
        void Enter(NamedState? previousState = null);
        void Pause();
        void Resume();
        void Exit();
    }
}