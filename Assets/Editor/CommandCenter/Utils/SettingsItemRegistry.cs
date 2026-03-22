using System.Collections.Generic;
using Editor.CommandCenter.Modules.Settings;
using Utils.SO.Settings.Screen;

namespace Editor.CommandCenter.Utils
{
    public static class SettingsItemRegistry
    {
        private static readonly Dictionary<SettingsItemType, ISettingsItemModule> Modules;

        static SettingsItemRegistry()
        {
            Modules = new Dictionary<SettingsItemType, ISettingsItemModule>
            {
                { SettingsItemType.Numeral, new FloatItemModule() },
                { SettingsItemType.Boolean, new BooleanItemModule() },
                { SettingsItemType.Enum, new EnumItemModule() },
                { SettingsItemType.InputKey, new InputKeyItemModule() }
            };
        }

        public static ISettingsItemModule Get(SettingsItemType type)
        {
            return Modules.GetValueOrDefault(type);
        }

        public static IEnumerable<SettingsItemType> GetTypes()
        {
            return Modules.Keys;
        }
    }
}