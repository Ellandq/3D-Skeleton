using System;
using System.Collections.Generic;
using System.Linq;
using GameInput;
using UnityEngine;
using Utils.SO.Input;
using Utils.SO.Settings.Utils.SO.Settings;

namespace Managers
{
    public class InputManager : ManagerBase<InputManager>
    {
        [Header("Input Settings")]
        [SerializeField] private AllowedInputKeys allowedInputKeys;
        [SerializeField] public InputAssignments defaultInputAssignments;
        private InputAssignments inputAssignments;

        [Header("Runtime")]
        private Dictionary<PlayerAction, ButtonInformationWrapper> _buttonInfoDict = new();
        private Dictionary<MouseKey, string> _allowedMouseButtons = new();
        private Dictionary<KeyCode, string> _allowedKeyboardButtons = new();
        private bool isWaitingForInput;
        private Action<string> _listener;

        [Header("Metadata info")]
        private const string InputPrefix = "Input/";

        protected override void Awake()
        {
            base.Awake();

            inputAssignments = Instantiate(defaultInputAssignments);
            
            _allowedMouseButtons = allowedInputKeys.categories
                .Where(c => c.type == InputKeyType.MouseKey)
                .SelectMany(c => c.keys)
                .Where(e => e.isAllowed)
                .ToDictionary(e => (MouseKey)e.intValue, e => e.value);
            
            _allowedKeyboardButtons = allowedInputKeys.categories
                .Where(c => c.type == InputKeyType.KeyCode)
                .SelectMany(c => c.keys)
                .Where(e => e.isAllowed)
                .ToDictionary(e => (KeyCode)e.intValue, e => e.value);

            LoadSettings();
        }

        private void Update()
        {
            foreach (var wrapper in _buttonInfoDict.Values)
                wrapper.UpdateState();
            
            if (!isWaitingForInput)
                return;

            foreach (var kvp in from kvp in _allowedMouseButtons
                     let state = Input.GetMouseButtonDown((int)kvp.Key)
                     where state
                     select kvp)
            {
                isWaitingForInput = false;
                _listener?.Invoke(kvp.Value);
                return;
            }
            
            foreach (var kvp in from kvp in _allowedKeyboardButtons
                     let state = Input.GetKeyDown(kvp.Key)
                     where state
                     select kvp)
            {
                isWaitingForInput = false;
                _listener?.Invoke(kvp.Value);
                return;
            }
        }

        public void Subscribe(
            PlayerAction action,
            Action<ButtonState> listener)
        {
            if (_buttonInfoDict.TryGetValue(action, out var wrapper))
                wrapper.Subscribe(listener);
        }

        public void Unsubscribe(
            PlayerAction action,
            Action<ButtonState> listener)
        {
            if (_buttonInfoDict.TryGetValue(action, out var wrapper))
                wrapper.Unsubscribe(listener);
        }

        public void WaitForInput(Action<string> listener)
        {
            isWaitingForInput = true;
            _listener = listener;
        }


        private void LoadSettings()
        {
            var loaded = new Dictionary<string, string>();

            foreach (var assignment in inputAssignments.assignments)
            {
                var baseKey = InputPrefix + assignment.settingName;
                var altKey = InputPrefix + assignment.settingName + "_Alt";

                assignment.baseValue =
                    SettingsManager.GetStringSetting(
                        baseKey,
                        assignment.baseValue);

                assignment.altValue =
                    SettingsManager.GetStringSetting(
                        altKey,
                        assignment.altValue);
            }

            AssignSettings();
        }


        private void AssignSettings()
        {
            var newDict = new Dictionary<PlayerAction, ButtonInformationWrapper>();

            foreach (var assignment in inputAssignments.assignments)
            {
                if (assignment.action == default)
                    continue;

                var old =
                    _buttonInfoDict.GetValueOrDefault(
                        assignment.action);

                newDict[assignment.action] =
                    new ButtonInformationWrapper(
                        assignment.baseValue,
                        assignment.altValue,
                        old);
            }

            _buttonInfoDict = newDict;
        }
    }


    internal class ButtonInformationWrapper
    {
        private bool _buttonState;

        private Action<ButtonState> _listeners;

        private readonly Func<bool> _inputCheckFunc;


        public ButtonInformationWrapper(
            string baseKey,
            string altKey,
            ButtonInformationWrapper old = null)
        {
            var baseInfo = new ButtonInformation(baseKey);
            var altInfo = new ButtonInformation(altKey);

            _buttonState =
                old?._buttonState ?? false;

            _listeners =
                old?._listeners;

            _inputCheckFunc =
                CreateInputCheckFunc(baseInfo, altInfo);
        }


        private static Func<bool> CreateInputCheckFunc(
            ButtonInformation baseInfo,
            ButtonInformation altInfo)
        {
            return () =>
                (baseInfo.IsValid &&
                 (baseInfo.IsKeyboard
                    ? Input.GetKey((KeyCode)baseInfo.KeyValue)
                    : Input.GetMouseButton(baseInfo.KeyValue)))
                ||
                (altInfo.IsValid &&
                 (altInfo.IsKeyboard
                    ? Input.GetKey((KeyCode)altInfo.KeyValue)
                    : Input.GetMouseButton(altInfo.KeyValue)));
        }


        public void UpdateState()
        {
            var old = _buttonState;

            _buttonState = _inputCheckFunc();

            if (old != _buttonState)
            {
                _listeners?.Invoke(
                    old
                        ? ButtonState.Up
                        : ButtonState.Down);
            }
        }


        public void Subscribe(Action<ButtonState> listener)
        {
            _listeners += listener;
        }


        public void Unsubscribe(Action<ButtonState> listener)
        {
            _listeners -= listener;
        }
    }


    internal struct ButtonInformation
    {
        public readonly bool IsKeyboard;
        public readonly int KeyValue;
        public readonly bool IsValid;


        public ButtonInformation(string key)
        {
            IsKeyboard = true;
            KeyValue = 0;
            IsValid = false;


            if (Enum.TryParse<KeyCode>(key, out var keyboard))
            {
                KeyValue = (int)keyboard;
                IsValid = true;
            }
            else if (Enum.TryParse<MouseKey>(key, out var mouse))
            {
                IsKeyboard = false;
                KeyValue = (int)mouse;
                IsValid = true;
            }
        }
    }
}