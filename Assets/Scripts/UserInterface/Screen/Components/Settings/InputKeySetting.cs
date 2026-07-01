using System;
using Managers;
using UnityEngine;
using UserInterface.Windows;
using Utils.Enum;
using Utils.SO;
using Utils.SO.Settings.Screen;

namespace UserInterface.Screen.Components.Settings
{
    public class InputKeySetting : SettingBase<string>
    {
        [Header("Object References")]
        [SerializeField] private Transform container;
        [SerializeField] private InputKeyButton baseButton;
        [SerializeField] private InputKeyButton alternateButton;

        [Header("Prefabs")]
        [SerializeField] private GameObject buttonPrefab;

        [Header("Runtime")]
        private bool blockInteractions;
        private string defaultBaseValue;
        private string startingBaseValue;
        private string defaultAltValue;
        private string startingAltValue;

        public override void Initialize(
            SettingsPageItemSO itemAsset,
            Action<string> onSelect,
            Action<(string key, string value)> onValueChange,
            Action<(string key, string value)> onValueReset,
            UIComponentState defaultState)
        {
            blockInteractions =
                asset.settingName == "Escape";

            base.Initialize(
                asset,
                onSelect,
                onValueChange,
                onValueReset,
                defaultState);

            CreateButtons(asset.AllowSecondaryInput);
            ApplyState(defaultState);
        }

        private void CreateButtons(bool allowSecondaryInput)
        {
            ClearButtons();

            baseButton = CreateButton();

            defaultBaseValue = SettingsManager.GetDefaultInputSetting(fullName);
            startingBaseValue = SettingsManager.GetStringSetting(fullName, "");

            if (!allowSecondaryInput)
            {
                ResetSetting();
                return;
            }
            alternateButton = CreateButton(true);

            defaultAltValue = SettingsManager.GetDefaultInputSetting(fullName + "_Alt");
            startingAltValue = SettingsManager.GetStringSetting(fullName + "_Alt", "");
            ResetSetting();
        }

        private InputKeyButton CreateButton(bool isAlt = false)
        {
            var button =
                Instantiate(
                    buttonPrefab,
                    container)
                .GetComponent<InputKeyButton>();

            button.Initialize(this, isAlt);

            return button;
        }

        private void ClearButtons()
        {
            for (var i = container.childCount - 1; i >= 0; i--)
            {
                var child = container.GetChild(i);

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEditor.Undo.DestroyObjectImmediate(child.gameObject);
                else
                    Destroy(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
        }

        public override void ChangeState(UIComponentState newState)
        {
            ApplyState(newState);
        }

        private void ApplyState(UIComponentState state)
        {
            if (blockInteractions)
                state = UIComponentState.Disabled;

            base.ChangeState(state);

            baseButton?.ChangeState(state);
            alternateButton?.ChangeState(state);
        }

        public void SettingChanged(string newValue, bool alt = false)
        {
            UIManager.Instance.DeactivateComponent(NamedWindow.InputAssignment);
            var sprite = SpriteManager.GetInputSprite(newValue);
            if (!alt)
            {
                if (newValue == startingAltValue)
                    _onValueReset?.Invoke((fullName, newValue));
                else _onValueChange?.Invoke((fullName, newValue));
                baseButton.UpdateIcon(sprite);
            }
            else
            {
                if (newValue == startingAltValue)
                    _onValueReset?.Invoke((fullName + "_Alt", newValue));
                else _onValueChange?.Invoke((fullName + "_Alt", newValue));
                alternateButton.UpdateIcon(sprite);
            }
        }

        public override void ResetSetting(bool toDefault = false)
        {
            baseButton.UpdateIcon(SpriteManager.GetInputSprite(toDefault ? defaultBaseValue : startingBaseValue));
            if (!alternateButton) return;
            alternateButton.UpdateIcon(SpriteManager.GetInputSprite(toDefault ? defaultAltValue : startingAltValue));
        }
    }
}