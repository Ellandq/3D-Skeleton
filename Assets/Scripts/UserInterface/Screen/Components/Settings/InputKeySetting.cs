using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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

        public override void Initialize(
            SettingsPageItemSO asset, 
            Action<string> onSelect,  
            Action<(string key, string value)> onValueChange,
            Action<(string key, string value)> onValueReset, 
            UIComponentState defaultState)
        {
            InitializePreview(asset.AllowSecondaryInput);
            base.Initialize(asset, onSelect, onValueChange, onValueReset, defaultState);
        }

        private void InitializePreview(bool allowSecondaryInput)
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
            baseButton = Instantiate(buttonPrefab, container).GetComponent<InputKeyButton>();
            baseButton.Initialize(this);
            if (!allowSecondaryInput) return;
            alternateButton = Instantiate(buttonPrefab, container).GetComponent<InputKeyButton>();
            alternateButton.Initialize(this);

        }

        public override void ChangeState(UIComponentState newState)
        {
            base.ChangeState(newState);

            baseButton.ChangeState(newState);
            alternateButton?.ChangeState(newState);
        }
    }
}