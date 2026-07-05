using System;
using Managers;
using UnityEngine;
using UserInterface.Windows;
using Utils.Data.Settings.Screen;
using Utils.Enum.UI;

namespace UserInterface.Screen.Components.Settings.Common
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
        private string currentBaseValue;
        private string currentAltValue;

        public override void Initialize(
            SettingsPageItemSO itemAsset,
            Action<string> onSelect,
            Action<(string key, string value)> onValueChange,
            Action<(string key, string value)> onValueReset,
            UIComponentState defaultState)
        {
            blockInteractions =
                itemAsset.settingName == "Escape";

            base.Initialize(
                itemAsset,
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
            currentBaseValue = startingBaseValue = SettingsManager.GetStringSetting(fullName, "");
            baseButton.UpdateIcon(SpriteManager.GetInputSprite(currentBaseValue));

            if (!allowSecondaryInput)
                return;
            
            alternateButton = CreateButton(true);

            defaultAltValue = SettingsManager.GetDefaultInputSetting(fullName + "_Alt");
            currentAltValue = startingAltValue = SettingsManager.GetStringSetting(fullName + "_Alt", "");
            alternateButton.UpdateIcon(SpriteManager.GetInputSprite(currentAltValue));
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
            UIManager.DeactivateComponent(NamedWindow.InputAssignment);
            var sprite = SpriteManager.GetInputSprite(newValue);

            if (!alt)
            {
                currentBaseValue = newValue;
                NotifyValueChanged(currentBaseValue, startingBaseValue);
                baseButton.UpdateIcon(sprite);
            }
            else
            {
                currentAltValue = newValue;
                NotifyValueChanged(currentAltValue, startingAltValue);
                alternateButton.UpdateIcon(sprite);
            }
        }

        public override void ResetSetting(bool toDefault = false)
        {
            var newBaseValue = toDefault ? defaultBaseValue : startingBaseValue;

            if (newBaseValue != currentBaseValue)
            {
                currentBaseValue = newBaseValue;
                baseButton.UpdateIcon(SpriteManager.GetInputSprite(newBaseValue));
                NotifyValueChanged(currentBaseValue, startingBaseValue);
            }

            if (!alternateButton)
                return;

            var newAltValue = toDefault ? defaultAltValue : startingAltValue;

            if (newAltValue == currentAltValue) return;
            currentAltValue = newAltValue;
            alternateButton.UpdateIcon(SpriteManager.GetInputSprite(newAltValue));
            NotifyValueChanged(currentAltValue, startingAltValue);
        }
        
        public override void UpdateStartValue()
        {
            startingBaseValue = currentBaseValue;

            if (alternateButton)
            {
                startingAltValue = currentAltValue;
            }
        }
    }
}