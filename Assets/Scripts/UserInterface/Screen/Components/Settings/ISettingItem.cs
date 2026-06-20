using System;
using Utils.Enum;
using Utils.SO.Settings.Screen;

namespace UserInterface.Screen.Components.Settings
{
    public interface ISettingItem
    {
        string GetId();
        void Initialize(SettingsPageItemSO asset, Action<string> onSelect, UIComponentState defaultState = UIComponentState.Enabled);
        void ChangeState(UIComponentState newState);
        void SelectItem();
        void DeselectItem();
    }
}