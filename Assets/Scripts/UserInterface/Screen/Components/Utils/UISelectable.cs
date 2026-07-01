using System;
using UnityEngine;
using Utils.Enum.UI;

namespace UserInterface.Screen.Components.Utils
{
    public abstract class UISelectable : MonoBehaviour
    {
        [Header("Settings")] 
        [SerializeField] protected UIComponentState state;
        [SerializeField] protected string id;

        [Header("Event")] 
        private Action<string> _onSelect;

        public string GetId() => id;

        public virtual void Initialize(Action<string> onSelect, UIComponentState defaultState = UIComponentState.Enabled)
        {
            id = string.IsNullOrEmpty(id)
                ? GUID.Generate().ToString()
                : id;
            _onSelect = onSelect;
        }

        public virtual void ChangeState(UIComponentState newState)
        {
            state = newState;
        }
        
        public virtual void SelectItem()
        {
            if (state == UIComponentState.Disabled)
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