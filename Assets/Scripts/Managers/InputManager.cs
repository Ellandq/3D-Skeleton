using System;
using System.Collections.Generic;
using System.Linq;
using GameInput;
using UnityEngine;
using Utils.SO.Settings.Utils.SO.Settings;

namespace Managers
{
    public class InputManager : ManagerBase<InputManager>
    {
        [Header("Input Settings")]
        [SerializeField] public InputAssignments defaultInputAssignments;
        private InputAssignments inputAssignments;

        private Dictionary<PlayerAction, ButtonInformationWrapper> _buttonInfoDict = new();

        private const string InputPrefix = "Input/";

        protected override void Awake()
        {
            base.Awake();

            inputAssignments = Instantiate(defaultInputAssignments);

            LoadSettings();
        }

        private void Update()
        {
            foreach (var wrapper in _buttonInfoDict.Values)
                wrapper.UpdateState();
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