using System;
using System.Globalization;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue;
        [SerializeField] private float step;
        
        public override void Initialize(
            SettingsPageItemSO asset, 
            Action<string> onSelect,  
            Action<(string key, float value)> onValueChange,
            Action<(string key, float value)> onValueReset, 
            UIComponentState defaultState)
        {
            wasChanged = false;
            base.Initialize(asset, onSelect, onValueChange, onValueReset, defaultState);
            slider.minValue = minValue = asset.MinValue;
            slider.maxValue = maxValue = asset.MaxValue;
            slider.value = value = asset.FloatDefaultValue;
            handleText.text = value.ToString(CultureInfo.InvariantCulture);
            step = asset.MinIncrement;
            slider.onValueChanged.AddListener(UpdateValue);
            releaseListener.onReleased += NotifyValueChanged;
        }

        public override void ChangeState(UIComponentState newState)
        {
            base.ChangeState(newState);

            var colors = UITheme.GetColors(newState);

            var lighterC = colors[UIColorType.Lighter];
            var lightC = colors[UIColorType.Light];
            var darkC = colors[UIColorType.Dark];

            handleFrame.color = lighterC;
            handleText.color = lighterC;
            fill.color = lighterC;
            handleBackground.color = lightC;
            sliderBackground.color = darkC;
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
    }
}