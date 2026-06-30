using System;
using Managers;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.Components;
using Utils.Enum;
using Utils.SO;
using Utils.SO.Settings.Screen;

namespace UserInterface.Screen.Components.Settings
{
    public abstract class SettingBase<T> : MonoBehaviour, ISettingItem<T>
    {
        [Header("Object References")]
        [SerializeField] protected TMP_Text settingName;
        [SerializeField] protected Image highlight;

        [Header("Settings")] 
        protected UIComponentState State;
        [SerializeField] protected string id;

        [Header("Event")] 
        private Action<string> _onSelect;
        protected Action<(string key, T value)> _onValueChange;
        protected Action<(string key, T value)> _onValueReset;

        public string GetId() => id;
        
        public virtual void Initialize(
            SettingsPageItemSO asset, 
            Action<string> onSelect, 
            Action<(string key, T value)> onValueChange,
            Action<(string key, T value)> onValueReset, 
            UIComponentState defaultState)
        {
            id = string.IsNullOrEmpty(asset.uniqueId)
                ? GUID.Generate().ToString()
                : asset.uniqueId;

            _onSelect = onSelect;
            settingName.text = asset.settingName;

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
        
        
    }
}