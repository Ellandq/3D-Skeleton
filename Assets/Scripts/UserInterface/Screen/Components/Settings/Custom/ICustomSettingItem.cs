using Model.Enum.Named;

namespace UserInterface.Screen.Components.Settings.Custom
{
    public interface ICustomSettingItem
    {
        NamedCustomSetting GetSettingType();
    }
}