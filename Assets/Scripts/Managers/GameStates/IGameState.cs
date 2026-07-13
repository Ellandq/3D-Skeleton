using Model.Enum.Named;

namespace Managers.GameStates
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