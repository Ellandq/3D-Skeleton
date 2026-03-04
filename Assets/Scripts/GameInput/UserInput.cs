using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameInput
{
    public abstract class UserInput<T> : MonoBehaviour
    {
        [Header("Events")] 
        protected Dictionary<PlayerAction, Action<ButtonState>> OnInputAction;
        protected Action OnAnyKeyPressed;

        [Header("Key Information")] 
        protected Dictionary<PlayerAction, T> ButtonAssignment;
        protected Dictionary<T, bool> ButtonStates;

        protected void Awake()
        {
            ButtonStates = new Dictionary<T, bool>();
            UpdateButtonStateDictionary();
            UpdateEventDictionaries(GetInputAssignment());
        }

        private void UpdateButtonStateDictionary()
        {
            ButtonStates = new Dictionary<T, bool>();

            foreach (var button in ButtonAssignment)
                ButtonStates.Add(button.Value, false);
        }

        private void UpdateEventDictionaries(List<(T button, PlayerAction action)> buttonActionList)
        {
            ButtonAssignment = buttonActionList
                .Select(n => new KeyValuePair<PlayerAction, T>(n.action, n.button))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            
            OnInputAction = new Dictionary<PlayerAction, Action<ButtonState>>();

            foreach (var tuple in buttonActionList)
                OnInputAction.Add(tuple.action, (state) => {});
        }

        public bool GetButtonState(T key) => ButtonStates[key];

        public bool GetButtonState(PlayerAction action) => ButtonStates[ButtonAssignment[action]];

        public void AddListenerOnInputAction(Action<ButtonState> actionToAdd, PlayerAction key) =>
            OnInputAction[key] += actionToAdd;
        
        public void AddListenerOnAnyKeyPressed (Action actionToAdd) => OnAnyKeyPressed += actionToAdd;
        
        public void RemoveListenerOnAnyKeyPressed (Action actionToAdd) => OnAnyKeyPressed -= actionToAdd;

        protected virtual List<(T button, PlayerAction action)> GetInputAssignment()
        {
            throw new NotImplementedException();
        }
    }
}