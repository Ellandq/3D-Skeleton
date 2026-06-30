using System;
using Managers;
using UnityEngine;
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

        private bool blockInteractions;

        public override void Initialize(
            SettingsPageItemSO asset,
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

            baseButton.UpdateIcon(
                SpriteManager.GetInputSprite(
                    SettingsManager.GetStringSetting(
                        fullName,
                        "")));

            if (!allowSecondaryInput) return;
            alternateButton = CreateButton();

            alternateButton.UpdateIcon(
                SpriteManager.GetInputSprite(
                    SettingsManager.GetStringSetting(
                        fullName + "_Alt",
                        "")));
        }


        private InputKeyButton CreateButton()
        {
            var button =
                Instantiate(
                    buttonPrefab,
                    container)
                .GetComponent<InputKeyButton>();

            button.Initialize(this);

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
    }
}