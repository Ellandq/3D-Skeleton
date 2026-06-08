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
    public abstract class SettingBase : MonoBehaviour, ISettingItem
    {
        [Header("Object References")]
        [SerializeField] protected TMP_Text settingName;
        [SerializeField] protected Image highlight;

        [Header("Settings")] 
        protected UIComponentState State;
        [SerializeField] protected string id;

        [Header("Event")] 
        private Action<string> _onSelect;

        public string GetId() => id;
        
        public virtual void Initialize(SettingsPageItemSO asset, Action<string> onSelect, UIComponentState defaultState)
        {
            id = string.IsNullOrEmpty(asset.uniqueId)
                ? GUID.Generate().ToString()
                : asset.uniqueId;

            _onSelect = onSelect;
            settingName.text = asset.settingName;

            ChangeState(defaultState);
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
            ChangeState(UIComponentState.Enabled);
        }
        
        
    }
}