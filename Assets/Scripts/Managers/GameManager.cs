using System;
using System.Collections.Generic;
using GameStates;
using UnityEngine;
using Utils.Enum;

namespace Managers
{
    public class GameManager : ManagerBase<GameManager>
    {
        [Header("State Management")]
        private Stack<IGameState> _stateStack = new();
        private bool _isTransitioning;

        #region INITIALIZATION

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) 
                return;
            
            if (_stateStack.Count == 0)
                PushState(NamedState.MainMenu);
            else if (_stateStack.TryPeek(out var state))
                state.Enter();
        }

        #endregion
        
        #region STATE MANAGEMENT

        public void PushState(NamedState newState, bool pause = true)
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

        public void PopState(bool resume = true)
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
            else
            {
                throw new InvalidOperationException("Cannot exit state when no states remain on the stack.");
            }
        }

        public void ChangeState(NamedState newState)
        {
            PopState(false);
            PushState(newState, false);
        }

        #endregion
    }
}
