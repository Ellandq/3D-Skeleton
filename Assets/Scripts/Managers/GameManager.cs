using System.Collections.Generic;
using Managers.GameStates;
using Model.Enum.Named;
using SaveAndLoad;
using UnityEngine;

namespace Managers
{
    [RequireComponent(typeof(GameLoader))]
    public class GameManager : ManagerBase<GameManager>
    {
        [Header("State Management")]
        private Stack<IGameState> _stateStack = new();
        private bool _isTransitioning;

        [Header("Save and Load Management")] 
        [SerializeField] public GameLoader gameLoader;
        public static GameLoader LoadHandle => Instance.gameLoader;

        #region INITIALIZATION

        protected override void Awake()
        {
            base.Awake();
            Time.timeScale = 0f;
        }

        protected void Start()
        {
            if (_stateStack.Count == 0)
                PushState(NamedState.MainMenu);
            else if (_stateStack.TryPeek(out var state))
                state.Enter();
        }

        #endregion
        
        #region STATE MANAGEMENT

        public static void PushState(NamedState newState, bool pause = true) => Instance.PushNewState(newState, pause);

        private void PushNewState(NamedState newState, bool pause = true)
        {
            if (pause && _stateStack.TryPeek(out var state))
            {
                if (state.Name == newState)
                {
                    return;
                }
                state.Pause();
            }

            var stateToEnter = GameStateFactory.Create(newState);
            stateToEnter.Enter();
            _stateStack.Push(stateToEnter);
        }

        public static void PopState(bool resume = true) => Instance.PopStateStack(resume);
        private void PopStateStack(bool resume = true)
        {
            if (!_stateStack.TryPop(out var stateToExit)) 
                return;
            
            stateToExit.Exit();
            
            if (!resume)
                return;
            
            if (_stateStack.TryPeek(out var lastState))
            {
                lastState.Resume();
            }
        }

        public static void ChangeState(NamedState newState)
        {
            PopState(false);
            PushState(newState, false);
        }

        #endregion
    }
}
