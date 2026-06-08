using System;
using System.Globalization;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Enum;
using Utils.SO;
using Utils.SO.Settings.Screen;

namespace UserInterface.Screen.Components.Settings
{
    public class FloatSetting : SettingBase
    {
        [Header("Object References")]
        [SerializeField] private Image handleBackground;
        [SerializeField] private Image handleFrame;
        [SerializeField] private TMP_Text handleText;
        [SerializeField] private Image fill;
        [SerializeField] private Image sliderBackground;
        [SerializeField] private Slider slider;

        [Header("Settings")] 
        [SerializeField] private float value;
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue;
        [SerializeField] private float step;
        
        public override void Initialize(SettingsPageItemSO asset, Action<string> onSelect, UIComponentState defaultState)
        {
            base.Initialize(asset, onSelect, defaultState);
            slider.minValue = minValue = asset.MinValue;
            slider.maxValue = maxValue = asset.MaxValue;
            slider.value = value = asset.FloatDefaultValue;
            handleText.text = value.ToString(CultureInfo.InvariantCulture);
            step = asset.MinIncrement;
            slider.onValueChanged.AddListener(UpdateValue);
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
    }
}