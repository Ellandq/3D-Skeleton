using System;
using Utils.Data.Settings.Screen;
using Utils.Enum.UI;

namespace UserInterface.Screen.Components.Settings.Common
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
        void ResetSetting(bool toDefault = false);
        void UpdateStartValue();
    }

    public interface ISettingItem<T> : ISettingItem
    {
        void Initialize(
            SettingsPageItemSO itemAsset,
            Action<string> onSelect,
            Action<(string key, T value)> onValueChange,
            Action<(string key, T value)> onValueReset,
            UIComponentState defaultState = UIComponentState.Enabled);
    }
}