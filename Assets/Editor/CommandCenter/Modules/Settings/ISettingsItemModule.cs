using System;
using UnityEngine.UIElements;
using Utils.SO.Settings.Screen;

namespace Editor.CommandCenter.Modules.Settings
{
    public interface ISettingsItemModule
    {
        SettingsItemType Type { get; }

        VisualElement CreateUI(SettingsPageItem item, Action onRemove);

        SettingsPageItem CreateInstance();
    }
}