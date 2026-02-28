using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using Editor.CommandCenter;
using Editor.CommandCenter.Utils;

namespace Editor.CommandCenter.Modules
{
    public class UIReadinessModule : IEditorModule
    {
        public string ModuleName => "UI Readiness";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Unknown;

        private ICommandCenterLogger _logger;

        private const string PrefabRoot = "Assets/Prefabs/UI/";
        
        private const string ScreenEnumPath =
            "Assets/Scripts/UserInterface/Screen/NamedScreen.cs";

        private const string HUDEnumPath =
            "Assets/Scripts/UserInterface/HUD/NamedHUD.cs";

        private const string OverlayEnumPath =
            "Assets/Scripts/UserInterface/Overlay/NamedOverlay.cs";

        public void Initialize(ICommandCenterLogger logger)
        {
            _logger = logger;
        }

        public VisualElement CreateContent()
        {
            return new Label("Validates UI enums, prefabs and addressable.");
        }

        #region PUBLIC

        public void Validate()
        {
            Status = ModuleStatus.Valid;

            ValidateCategory(
                typeof(UserInterface.Screen.ScreenBase),
                typeof(UserInterface.Screen.NamedScreen),
                "Screen",
                false);

            ValidateCategory(
                typeof(UserInterface.HUD.HUDBase),
                typeof(UserInterface.HUD.NamedHUD),
                "HUD",
                false);

            ValidateCategory(
                typeof(UserInterface.Overlay.OverlayBase),
                typeof(UserInterface.Overlay.NamedOverlay),
                "Overlay",
                false);
        }

        public void Enforce()
        {
            Status = ModuleStatus.Valid;

            EnforceCategory(
                typeof(UserInterface.Screen.ScreenBase),
                ScreenEnumPath,
                "UserInterface.Screen",
                "NamedScreen",
                "Screen");

            EnforceCategory(
                typeof(UserInterface.HUD.HUDBase),
                HUDEnumPath,
                "UserInterface.HUD",
                "NamedHUD",
                "HUD");

            EnforceCategory(
                typeof(UserInterface.Overlay.OverlayBase),
                OverlayEnumPath,
                "UserInterface.Overlay",
                "NamedOverlay",
                "Overlay");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #endregion

        #region CORE VALIDATION

        private void ValidateCategory(
            Type baseType,
            Type enumType,
            string folder,
            bool enforce)
        {
            var implementations = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    baseType.IsAssignableFrom(t) &&
                    !t.IsAbstract &&
                    !t.IsInterface);

            var enumNames = Enum.GetNames(enumType).ToList();

            foreach (var impl in implementations)
            {
                var name = impl.Name;

                if (folder.Equals("Screen", StringComparison.OrdinalIgnoreCase) && name.EndsWith("Screen"))
                    name = name[..^"Screen".Length];
                else if (folder.Equals("HUD", StringComparison.OrdinalIgnoreCase) && name.EndsWith("HUD"))
                    name = name[..^"HUD".Length];
                else if (folder.Equals("Overlay", StringComparison.OrdinalIgnoreCase) && name.EndsWith("Overlay"))
                    name = name[..^"Overlay".Length];

                if (!enumNames.Contains(name))
                {
                    _logger.LogError($"{folder}: Missing enum for {name}");
                    Status = ModuleStatus.Error;
                    continue;
                }

                var prefabPath = $"{PrefabRoot}{folder}/{impl.Name}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (!prefab)
                {
                    _logger.LogWarning($"{folder}: Missing prefab {prefabPath}");
                    if (Status != ModuleStatus.Error)
                        Status = ModuleStatus.Warning;
                    continue;
                }

                ValidateAddressable(prefabPath, name, folder, enforce);
            }
        }

        #endregion

        #region ENUM ENFORCEMENT

        private void EnforceCategory(
            Type baseType,
            string enumPath,
            string enumNamespace,
            string enumName,
            string folder)
        {
            var types = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    baseType.IsAssignableFrom(t) &&
                    !t.IsInterface &&
                    !t.IsAbstract)
                .Select(t =>
                {
                    var name = t.Name;
                    if (folder.Equals("Screen", StringComparison.OrdinalIgnoreCase) && name.EndsWith("Screen"))
                        name = name[..^"Screen".Length];
                    else if (folder.Equals("HUD", StringComparison.OrdinalIgnoreCase) && name.EndsWith("HUD"))
                        name = name[..^"HUD".Length];
                    else if (folder.Equals("Overlay", StringComparison.OrdinalIgnoreCase) && name.EndsWith("Overlay"))
                        name = name[..^"Overlay".Length];

                    return name;
                })
                .OrderBy(n => n)
                .ToArray();

            GenerateEnum(enumPath, enumNamespace, enumName, types);

            foreach (var name in types)
            {
                var prefabPath = $"{PrefabRoot}{folder}/{name}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (!prefab)
                {
                    _logger.LogWarning($"{folder}: Missing prefab {prefabPath}");
                    Status = ModuleStatus.Warning;
                    continue;
                }

                ValidateAddressable(prefabPath, name, folder, true);
            }

            _logger.Log($"{folder}: Enum synchronized.");
        }
        
        private void GenerateEnum(
            string path,
            string enumNamespace,
            string enumName,
            string[] values)
        {
            var builder = new System.Text.StringBuilder();

            builder.AppendLine("// <auto-generated>");
            builder.AppendLine("// This file is auto-generated. Do not edit manually.");
            builder.AppendLine("// </auto-generated>");
            builder.AppendLine();
            builder.AppendLine($"namespace {enumNamespace}");
            builder.AppendLine("{");
            builder.AppendLine($"    public enum {enumName}");
            builder.AppendLine("    {");

            for (var i = 0; i < values.Length; i++)
            {
                builder.Append("        " + values[i]);
                if (i < values.Length - 1)
                    builder.Append(",");
                builder.AppendLine();
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");

            WriteIfChanged(path, builder.ToString());
        }
        
        private void WriteIfChanged(string path, string newContent)
        {
            if (File.Exists(path))
            {
                var current = File.ReadAllText(path);
                if (current == newContent)
                {
                    _logger.Log("No enum changes required: " + Path.GetFileName(path));
                    return;
                }
            }

            File.WriteAllText(path, newContent);
            _logger.Log("Updated enum: " + Path.GetFileName(path));
        }

        #endregion

        #region ADDRESSABLE ENFORCEMENT

        private void ValidateAddressable(
            string assetPath,
            string expectedAddress,
            string folder,
            bool enforce)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (!settings)
            {
                _logger.LogWarning("Addressable not configured.");
                Status = ModuleStatus.Warning;
                return;
            }

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = settings.FindAssetEntry(guid);

            if (entry == null)
            {
                if (enforce)
                {
                    entry = settings.CreateOrMoveEntry(
                        guid,
                        settings.DefaultGroup);

                    entry.address = expectedAddress;
                    _logger.Log($"{folder}: Marked Addressable {expectedAddress}");
                }
                else
                {
                    _logger.LogWarning($"{folder}: Prefab not Addressable");
                    Status = ModuleStatus.Warning;
                }

                return;
            }

            if (entry.address == expectedAddress) return;
            if (enforce)
            {
                entry.address = expectedAddress;
                _logger.Log($"{folder}: Fixed address {expectedAddress}");
            }
            else
            {
                _logger.LogWarning(
                    $"{folder}: Address mismatch ({entry.address})");
                Status = ModuleStatus.Warning;
            }
        }

        #endregion
    }
}