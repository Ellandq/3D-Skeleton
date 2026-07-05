using System;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Data;
using Utils.Data.Settings.Screen;
using Utils.Enum.UI;

namespace UserInterface.Screen.Components.Settings.Common
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
        private int startingValue;
        
        public Transform GetConditionalParent() => conditionalSettingsContentParent;

        public override void Initialize(
            SettingsPageItemSO itemAsset,
            Action<string> onSelect,
            Action<(string key, int value)> onValueChange,
            Action<(string key, int value)> onValueReset,
            UIComponentState defaultState)
        {
            base.Initialize(itemAsset, onSelect, onValueChange, onValueReset, defaultState);
            
            startingValue = SettingsManager.GetIntSetting(itemAsset.fullName, itemAsset.BoolDefaultValue ? 1 : 0);
            isOn = startingValue == 1;

            if (isOn && itemAsset.ConditionalItems is { Count: > 0 })
            {
                conditionalSettingsObject.SetActive(true);
            }

            button.onClick.AddListener(ChangeStatus);

            UpdateStatus();
            DeselectItem();
        }

        public override void ChangeState(UIComponentState newState)
        {
            base.ChangeState(newState);

            var colors = UITheme.GetColors(newState);

            text.color = colors[UIColorType.Lighter];
            frame.color = colors[UIColorType.Lighter];
            background.color = colors[UIColorType.Light];

            button.interactable = newState != UIComponentState.Disabled;
        }

        private void UpdateStatus()
        {
            frame.sprite = isOn ? onSprite : offSprite;
            text.text = isOn ? "On" : "Off";
        }

        private void ChangeStatus()
        {
            isOn = !isOn;
            UpdateStatus();

            if (conditionalSettingsContentParent.childCount > 0)
            {
                conditionalSettingsObject.SetActive(isOn);

                if (_rootLayout)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_rootLayout);
                }
            }

            NotifyValueChanged(isOn ? 1 : 0, startingValue);
        }

        public void SetRootLayout(RectTransform root)
        {
            _rootLayout = root;
        }
        
        public override void ResetSetting(bool toDefault = false)
        {
            var newValue = toDefault
                ? asset.BoolDefaultValue
                : startingValue == 1;

            if (isOn == newValue)
                return;

            isOn = newValue;

            UpdateStatus();

            if (conditionalSettingsContentParent.childCount > 0)
            {
                conditionalSettingsObject.SetActive(isOn);

                if (_rootLayout)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_rootLayout);
            }

            NotifyValueChanged(isOn ? 1 : 0, startingValue);
        }
        
        public override void UpdateStartValue()
        {
            startingValue = isOn ? 1 : 0;
        }
    }
}