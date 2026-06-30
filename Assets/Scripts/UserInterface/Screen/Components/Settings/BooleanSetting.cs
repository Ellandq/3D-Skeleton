using System;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Enum;
using Utils.SO;
using Utils.SO.Settings.Screen;

namespace UserInterface.Screen.Components.Settings
{
    public class BooleanSetting : SettingBase<int>
    {
        [Header("Object References")]
        [SerializeField] private GameObject conditionalSettingsObject;
        [SerializeField] private Transform conditionalSettingsContentParent;
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Image frame;
        [SerializeField] private TMP_Text text;
        private RectTransform _rootLayout;

        [Header("Sprites")] 
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Sprite offSprite;

        [Header("Settings")] 
        [SerializeField] private bool isOn;
        [SerializeField] private bool wasChanged;
        
        public Transform GetConditionalParent() => conditionalSettingsContentParent;

        public override void Initialize(
            SettingsPageItemSO asset, 
            Action<string> onSelect, 
            Action<(string key, int value)> onValueChange,
            Action<(string key, int value)> onValueReset, 
            UIComponentState defaultState)
        {
            base.Initialize(asset, onSelect, onValueChange, onValueReset, defaultState);
            wasChanged = false;
            isOn = asset.BoolDefaultValue;
            UpdateStatus();
            if (isOn)
            {
                if (isOn && asset.ConditionalItems != null && asset.ConditionalItems.Count != 0)
                {
                    conditionalSettingsObject.SetActive(true);
                }
            }
            
            button.onClick.AddListener(ChangeStatus);
            UpdateStatus();
            DeselectItem();
        }

        public override void ChangeState(UIComponentState newState)
        {
            base.ChangeState(newState);

            var colors = UITheme.GetColors(newState);

            var lighterC = colors[UIColorType.Lighter];
            var lightC = colors[UIColorType.Light];

            text.color = lighterC;
            frame.color = lighterC;
            background.color = lightC;

            button.interactable = newState != UIComponentState.Disabled;
        }

        private void UpdateStatus()
        {
            frame.sprite = isOn
                ? onSprite
                : offSprite;
            text.text = isOn
                ? "On"
                : "Off";
        }
        
        private void ChangeStatus()
        {
            isOn = !isOn;
            UpdateStatus();

            if (conditionalSettingsContentParent.childCount == 0)
                return;
            conditionalSettingsObject.SetActive(isOn);

            if (_rootLayout)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_rootLayout);
            }

            if (wasChanged)
            {
                wasChanged = false;
                _onValueReset?.Invoke((fullName, isOn ? 1 : 0));
            }
            else
            {
                _onValueChange?.Invoke((fullName, isOn ? 1 : 0));
                wasChanged = true;
            }
        }
        
        public void SetRootLayout(RectTransform root)
        {
            _rootLayout = root;
        }
    }
}