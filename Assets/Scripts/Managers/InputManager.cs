using System;
using System.Collections.Generic;
using GameInput;
using UnityEngine;
using Utils.SO.Settings;

namespace Managers
{
    public class InputManager : ManagerBase<InputManager>
    {
        [Header("Input Settings")]
        [SerializeField] public InputAssignments defaultInputAssignments;
        
        [Header("Runtime Dictionary")]
        private Dictionary<PlayerAction, ButtonInformationWrapper> _buttonInfoDict = new();
        
        private void Update()
        {
            foreach (var wrapper in _buttonInfoDict.Values)
            {
                wrapper.UpdateState();
            }
        }

        public void Subscribe(PlayerAction action, Action<ButtonState> listener) =>
            _buttonInfoDict[action].Subscribe(listener);
        
        public void Unsubscribe(PlayerAction action, Action<ButtonState> listenerToRemove) =>
            _buttonInfoDict[action].Unsubscribe(listenerToRemove);

        #region SETTINGS

        public void SaveSettings(Dictionary<string, string> dict)
        {
            foreach (var kvp in dict)
            {
                PlayerPrefs.SetString(kvp.Key, kvp.Value);
            } 
            defaultInputAssignments.FromDictionary(dict);
            AssignSettings();
        }

        public void LoadSettings()
        {
            var defaultDict = defaultInputAssignments.AsDictionary();
            var newDict = new Dictionary<string, string>();
            
            foreach (string key in Enum.GetValues(typeof(PlayerAction)))
            {
                var altKey = key + "_Alt";
                newDict.Add(key, PlayerPrefs.GetString(key, defaultDict[key]));
                newDict.Add(altKey, PlayerPrefs.GetString(altKey, defaultDict[altKey]));
            }
            
            defaultInputAssignments.FromDictionary(newDict);
            AssignSettings();
        }

        private void AssignSettings()
        {
            var dict = defaultInputAssignments.AsSimpleDictionary();
            var newDict = new Dictionary<PlayerAction, ButtonInformationWrapper>();

            foreach (var kvp in dict)
            {
                var newButtonInfo = new ButtonInformationWrapper(
                    kvp.Value.baseValue, 
                    kvp.Value.altValue, 
                    _buttonInfoDict.GetValueOrDefault(kvp.Key, null)
                );
                newDict.Add(kvp.Key, newButtonInfo);
            }
            _buttonInfoDict.Clear();
            _buttonInfoDict = newDict;
        }

        #endregion
    }

    internal class ButtonInformationWrapper
    {
        private bool _buttonState;
        
        private Action<ButtonState> _onInputChangeAction;

        private readonly Func<bool> _inputCheckFunc;

        public ButtonInformationWrapper(
            string newBaseValue,
            string newAltValue,
            ButtonInformationWrapper original = null
        )
        {
            var baseInfo = new ButtonInformation(newBaseValue);
            var altInfo = new ButtonInformation(newAltValue);
            _buttonState = original?._buttonState ?? false;
            _onInputChangeAction = original?._onInputChangeAction;
            _inputCheckFunc = CreateInputCheckFunc(baseInfo, altInfo);
        }

        private static Func<bool> CreateInputCheckFunc(ButtonInformation baseInfo, ButtonInformation altInfo)
        {
            return () =>
                (baseInfo.IsValid && (baseInfo.IsKeyboard
                    ? Input.GetKey((KeyCode)baseInfo.KeyValue)
                    : Input.GetMouseButton(baseInfo.KeyValue)))
                || (altInfo.IsValid && (altInfo.IsKeyboard
                    ? Input.GetKey((KeyCode)altInfo.KeyValue)
                    : Input.GetMouseButton(altInfo.KeyValue)));
        }

        public void UpdateState()
        {
            var oldState = _buttonState;
            _buttonState = _inputCheckFunc();

            if (oldState ^ _buttonState)
            {
                Invoke(oldState ? ButtonState.Up : ButtonState.Down);
            }
        }

        private void Invoke(ButtonState state) => _onInputChangeAction?.Invoke(state);
        public void Subscribe(Action<ButtonState> listener) => _onInputChangeAction += listener;
        public void Unsubscribe(Action<ButtonState> listenerToRemove) => _onInputChangeAction -= listenerToRemove;
    }

    internal struct ButtonInformation
    {
        public readonly bool IsKeyboard;
        public readonly int KeyValue;
        public readonly bool IsValid;

        public ButtonInformation(string key)
        {
            var isKeyboard = true;
            var keyValue = 0;
            IsValid = false;

            if (Enum.TryParse<KeyCode>(key, out var keyboardKey))
            {
                keyValue = (int)keyboardKey;
                IsValid = true;
            }
            else if (Enum.TryParse<MouseKey>(key, out var mouseKey))
            {
                isKeyboard = false;
                keyValue = (int)mouseKey;
                IsValid = true;
            }

            IsKeyboard = isKeyboard;
            KeyValue = keyValue;
        }
    }
}