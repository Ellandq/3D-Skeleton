using System;
using Utils.Enum;
using Utils.SO.Settings.Screen;

namespace UserInterface.Screen.Components.Settings
{
    public interface ISettingItem
    {
        string GetId();

        void Initialize(
            SettingsPageItemSO asset,
            Action<string> onSelect,
            Action<string, object> onValueChange,
            Action<string, object> onValueReset,
            UIComponentState defaultState = UIComponentState.Enabled);

        void ChangeState(UIComponentState newState);
        void SelectItem();
        void DeselectItem();
    }

    public interface ISettingItem<T> : ISettingItem
    {
        void Initialize(
            SettingsPageItemSO asset,
            Action<string> onSelect,
            Action<(string key, T value)> onValueChange,
            Action<(string key, T value)> onValueReset,
            UIComponentState defaultState = UIComponentState.Enabled);
    }
}