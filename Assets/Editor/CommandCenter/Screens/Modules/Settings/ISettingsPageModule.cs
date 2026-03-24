using UnityEngine.UIElements;

namespace Editor.CommandCenter.Screens.Modules.Settings
{
    public interface ISettingsPageModule
    {
        VisualElement CreateUI();
    }
}