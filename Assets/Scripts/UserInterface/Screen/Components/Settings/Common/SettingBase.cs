using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Enum.UI;
using Utils.SO;
using Utils.SO.Settings.Screen;

namespace UserInterface.Screen.Components.Settings.Common
{
    public abstract class SettingBase<T> : MonoBehaviour, ISettingItem<T>
    {
        [Header("Object References")]
        [SerializeField] protected TMP_Text settingName;
        [SerializeField] protected Image highlight;

        [Header("Settings")] 
        protected UIComponentState State;
        protected SettingsPageItemSO asset;
        [SerializeField] protected string id;
        [SerializeField] protected string fullName;

        [Header("Event")] 
        private Action<string> _onSelect;
        protected Action<(string key, T value)> _onValueChange;
        protected Action<(string key, T value)> _onValueReset;

        public string GetId() => id;
        
        public virtual void Initialize(
            SettingsPageItemSO itemAsset, 
            Action<string> onSelect, 
            Action<(string key, T value)> onValueChange,
            Action<(string key, T value)> onValueReset, 
            UIComponentState defaultState)
        {
            asset = itemAsset;
            id = string.IsNullOrEmpty(asset.uniqueId)
                ? GUID.Generate().ToString()
                : asset.uniqueId;

            _onSelect = onSelect;
            settingName.text = asset.settingName;
            fullName = asset.fullName;
            
            _onValueChange = onValueChange;
            _onValueReset = onValueReset;

            ChangeState(defaultState);
        }
        
        void ISettingItem.Initialize(
            SettingsPageItemSO asset,
            Action<string> onSelect,
            Action<string, object> onValueChange,
            Action<string, object> onValueReset,
            UIComponentState defaultState)
        {
            Initialize(
                asset,
                onSelect,
                x => onValueChange(x.key, x.value),
                x => onValueReset(x.key, x.value),
                defaultState);
        }
        
        protected void NotifyValueChanged(T currentValue, T startingValue)
        {
            if (ValuesEqual(currentValue, startingValue))
            {
                _onValueReset?.Invoke((fullName, currentValue));
            }
            else
            {
                _onValueChange?.Invoke((fullName, currentValue));
            }
        }

        protected virtual bool ValuesEqual(T a, T b)
        {
            return EqualityComparer<T>.Default.Equals(a, b);
        }

        public virtual void ChangeState(UIComponentState newState)
        {
            State = newState;
            highlight.enabled = State == UIComponentState.Selected;
            settingName.color = UITheme.GetColor(State, UIColorType.Lighter);
        }

        public virtual void SelectItem()
        {
            if (State == UIComponentState.Disabled)
            {
                return;
            }
            _onSelect?.Invoke(id);
            ChangeState(UIComponentState.Selected);
        }

        public virtual void DeselectItem()
        {
            if (State == UIComponentState.Disabled)
            {
                return;
            }
            ChangeState(UIComponentState.Enabled);
        }

        public virtual void ResetSetting(bool toDefault = false)
        {
            
        }

        public virtual void UpdateStartValue()
        {
            
        }
    }
}