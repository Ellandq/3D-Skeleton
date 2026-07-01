using System;
using System.Globalization;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.Components;
using Utils.Enum;
using Utils.SO;
using Utils.SO.Settings.Screen;

namespace UserInterface.Screen.Components.Settings
{
    public class FloatSetting : SettingBase<float>
    {
        [Header("Object References")]
        [SerializeField] private Image handleBackground;
        [SerializeField] private Image handleFrame;
        [SerializeField] private TMP_Text handleText;
        [SerializeField] private Image fill;
        [SerializeField] private Image sliderBackground;
        [SerializeField] private Slider slider;
        [SerializeField] private SliderReleaseListener releaseListener;

        [Header("Settings")]
        [SerializeField] private float startValue;
        [SerializeField] private bool wasChanged;
        [SerializeField] private float value;
        [SerializeField] private float step;

        public override void Initialize(
            SettingsPageItemSO itemAsset,
            Action<string> onSelect,
            Action<(string key, float value)> onValueChange,
            Action<(string key, float value)> onValueReset,
            UIComponentState defaultState)
        {
            wasChanged = false;

            base.Initialize(itemAsset, onSelect, onValueChange, onValueReset, defaultState);

            slider.minValue = itemAsset.MinValue;
            slider.maxValue = itemAsset.MaxValue;
            slider.value = value = SettingsManager.GetFloatSetting(itemAsset.fullName, itemAsset.FloatDefaultValue);
            step = itemAsset.MinIncrement;

            handleText.text = value.ToString(CultureInfo.InvariantCulture);

            slider.onValueChanged.AddListener(UpdateValue);
            releaseListener.onReleased += NotifyValueChanged;
        }

        public override void ChangeState(UIComponentState newState)
        {
            base.ChangeState(newState);

            var colors = UITheme.GetColors(newState);

            handleFrame.color = colors[UIColorType.Lighter];
            handleText.color = colors[UIColorType.Lighter];
            fill.color = colors[UIColorType.Lighter];
            handleBackground.color = colors[UIColorType.Light];
            sliderBackground.color = colors[UIColorType.Dark];
        }

        private void UpdateValue(float newValue)
        {
            value = Mathf.Round(newValue / step) * step;
            slider.SetValueWithoutNotify(value);
            handleText.text = value.ToString(CultureInfo.InvariantCulture);
        }

        private void NotifyValueChanged()
        {
            if (Mathf.Approximately(value, startValue))
            {
                if (!wasChanged)
                    return;

                _onValueReset?.Invoke((fullName, value));
                wasChanged = false;
                return;
            }

            _onValueChange?.Invoke((fullName, value));
            wasChanged = true;
        }
        
        public override void ResetSetting(bool toDefault = false)
        {
            var newValue = toDefault ? asset.FloatDefaultValue : startValue;
            if (Mathf.Approximately(value, newValue))
                return;
            UpdateValue(newValue);
            _onValueChange?.Invoke((fullName, value));
        }
    }
}