using System.IO;
using System.Linq;
using UnityEditor;
using Utils.Data.Settings.Screen;

namespace Editor.CommandCenter.Screens.SettingsEnforcement
{
    public static class SettingsEnforcerGenerator
    {
        private const string Folder =
            "Assets/Scripts/Settings/Enforcers";


        public static void Create(
            string name,
            string key,
            SettingsItemType type,
            ICommandCenterLogger logger)
        {
            Directory.CreateDirectory(Folder);


            var className =
                Sanitize(name) +
                "Enforcer";


            var valueType =
                GetValueType(type);


            var path =
                $"{Folder}/{className}.cs";


            if (File.Exists(path))
            {
                logger.LogWarning(
                    $"{className} already exists.");

                return;
            }


            var code =
$@"using Settings;

namespace Settings.Enforcers
{{
    public class {className} :
        ISettingEnforcer<{valueType}>
    {{
        public string GetKey()
        {{
            return ""{key}"";
        }}


        public void Enforce(
            string fullName,
            {valueType} value)
        {{
        }}

        void ISettingEnforcer.Enforce(
            string fullName,
            object value)
        {{
            Enforce(
                fullName,
                ({valueType})value);
        }}
    }}
}}";


            File.WriteAllText(
                path,
                code);


            AssetDatabase.Refresh();


            logger.Log(
                $"Created {className}");
        }


        private static string GetValueType(
            SettingsItemType type)
        {
            return type switch
            {
                SettingsItemType.Float =>
                    "float",

                SettingsItemType.Enum =>
                    "int",

                SettingsItemType.Boolean =>
                    "int",

                SettingsItemType.InputKey =>
                    "string",

                SettingsItemType.Custom =>
                    "string",

                _ =>
                    "string"
            };
        }


        private static string Sanitize(
            string input)
        {
            input = Path.GetInvalidFileNameChars().Aggregate(input, (current, c) => current.Replace(c, '_'));


            return input.Replace(" ", "");
        }
    }
}