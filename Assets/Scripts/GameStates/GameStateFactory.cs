using System;

namespace GameStates
{
    public static class GameStateFactory
    {
        public static IGameState Create(NamedState state)
        {
            IGameState instance = state switch
            {
                NamedState.MainMenu => new MainMenuState(),
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown NamedState")
            };

            return instance;
        }
    }
}