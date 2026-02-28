using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.AddressableAssets;
using Editor.CommandCenter.Utils;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UserInterface.HUD;
using UserInterface.Overlay;
using UserInterface.Screen;

namespace Editor.CommandCenter.Modules
{
    public class UIReadinessModule : IEditorModule
    {
        public string ModuleName => "UI Readiness";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Unknown;

        private ICommandCenterLogger _logger;

        private const string PrefabRoot = "Assets/Prefabs/UI/";
        private const string ScreenEnumPath = "Assets/Scripts/UserInterface/Screen/NamedScreen.cs";
        private const string HUDEnumPath = "Assets/Scripts/UserInterface/HUD/NamedHUD.cs";
        private const string OverlayEnumPath = "Assets/Scripts/UserInterface/Overlay/NamedOverlay.cs";

        public void Initialize(ICommandCenterLogger logger) => _logger = logger;

        public VisualElement CreateContent() => new Label("Validates UI enums, prefabs, and addressables.");

        #region PUBLIC

        public void Validate()
        {
            Status = ModuleStatus.Valid;

            ValidateCategory(typeof(ScreenBase), typeof(NamedScreen), "Screen");
            ValidateCategory(typeof(HUDBase), typeof(NamedHUD), "HUD");
            ValidateCategory(typeof(OverlayBase), typeof(NamedOverlay), "Overlay");
        }

        public void Enforce()
        {
            Status = ModuleStatus.Valid;

            EnforceCategory(typeof(ScreenBase), ScreenEnumPath, "UserInterface.Screen", "NamedScreen", "Screen");
            EnforceCategory(typeof(HUDBase), HUDEnumPath, "UserInterface.HUD", "NamedHUD", "HUD");
            EnforceCategory(typeof(OverlayBase), OverlayEnumPath, "UserInterface.Overlay", "NamedOverlay", "Overlay");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #endregion

        #region VALIDATION (READ-ONLY)

        private void ValidateCategory(Type baseType, Type enumType, string folder)
        {
            var implementations = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

            var enumNames = Enum.GetNames(enumType).ToList();

            foreach (var impl in implementations)
            {
                var enumName = impl.Name;
                var prefabName = impl.Name;

                if (folder.Equals("Screen", StringComparison.OrdinalIgnoreCase) && enumName.EndsWith("Screen"))
                    enumName = enumName[..^"Screen".Length];
                else if (folder.Equals("HUD", StringComparison.OrdinalIgnoreCase) && enumName.EndsWith("HUD"))
                    enumName = enumName[..^"HUD".Length];
                else if (folder.Equals("Overlay", StringComparison.OrdinalIgnoreCase) && enumName.EndsWith("Overlay"))
                    enumName = enumName[..^"Overlay".Length];

                if (!enumNames.Contains(enumName))
                {
                    _logger.LogError($"{folder}: Missing enum for {enumName}");
                    Status = ModuleStatus.Error;
                }

                var prefabPath = $"{PrefabRoot}{folder}/{prefabName}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (!prefab)
                {
                    _logger.LogWarning($"{folder}: Missing prefab {prefabPath}");
                    if (Status != ModuleStatus.Error)
                        Status = ModuleStatus.Warning;
                    continue;
                }

                var settings = AddressableAssetSettingsDefaultObject.Settings;
                if (!settings) continue;

                var guid = AssetDatabase.AssetPathToGUID(prefabPath);
                var entry = settings.FindAssetEntry(guid);

                if (entry == null)
                {
                    _logger.LogWarning($"{folder}: Prefab exists but is NOT marked as Addressable ({prefabName})");
                    if (Status != ModuleStatus.Error)
                        Status = ModuleStatus.Warning;
                }
                else if (entry.address != enumName)
                {
                    _logger.LogWarning($"{folder}: Addressable has wrong address ({entry.address}, expected {enumName})");
                    if (Status != ModuleStatus.Error)
                        Status = ModuleStatus.Warning;
                }
            }
        }

        #endregion

        #region ENFORCEMENT

        private void EnforceCategory(Type baseType, string enumPath, string enumNamespace, string enumName, string folder)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .Select(t =>
                {
                    var strippedEnumName = t.Name;
                    var fullPrefabName = t.Name;

                    if (folder.Equals("Screen", StringComparison.OrdinalIgnoreCase) && strippedEnumName.EndsWith("Screen"))
                        strippedEnumName = strippedEnumName[..^"Screen".Length];
                    else if (folder.Equals("HUD", StringComparison.OrdinalIgnoreCase) && strippedEnumName.EndsWith("HUD"))
                        strippedEnumName = strippedEnumName[..^"HUD".Length];
                    else if (folder.Equals("Overlay", StringComparison.OrdinalIgnoreCase) && strippedEnumName.EndsWith("Overlay"))
                        strippedEnumName = strippedEnumName[..^"Overlay".Length];

                    return new { EnumName = strippedEnumName, PrefabName = fullPrefabName };
                })
                .OrderBy(x => x.EnumName)
                .ToArray();

            EnumSynchronizer.Synchronize(enumPath, enumNamespace, enumName, types.Select(x => x.EnumName).ToArray(), _logger);

            foreach (var entry in types)
            {
                var prefabPath = $"{PrefabRoot}{folder}/{entry.PrefabName}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (!prefab)
                {
                    _logger.LogWarning($"{folder}: Missing prefab {prefabPath}");
                    Status = ModuleStatus.Warning;
                    continue;
                }

                EnsureAddressableExists(prefabPath, entry.EnumName, folder);
            }

            _logger.Log($"{folder}: Enum synchronized and addressables ensured.");
        }

        #endregion

        #region ADDRESSABLE

        private void EnsureAddressableExists(string prefabPath, string address, string folder)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (!settings)
            {
                _logger.LogWarning("Addressable not configured.");
                Status = ModuleStatus.Warning;
                return;
            }

            var groupName = folder switch
            {
                "Screen" => "NamedScreen",
                "HUD" => "NamedHUD",
                "Overlay" => "NamedOverlay",
                _ => "Default"
            };

            var group = settings.groups.FirstOrDefault(g => g.Name == groupName);
            if (!group)
            {
                group = settings.CreateGroup(groupName, false, false, false, null, typeof(BundledAssetGroupSchema));
                _logger.Log($"Created Addressable group '{groupName}' for folder {folder}");
            }

            var guid = AssetDatabase.AssetPathToGUID(prefabPath);
            var entry = settings.FindAssetEntry(guid);

            if (entry == null)
            {
                entry = settings.CreateOrMoveEntry(guid, group);
                entry.address = address;
                _logger.Log($"{folder}: Marked Addressable {address} in group {groupName}");
            }
            else
            {
                if (entry.address != address)
                {
                    entry.address = address;
                    _logger.Log($"{folder}: Fixed address {address}");
                }

                if (entry.parentGroup == group) return;
                settings.MoveEntry(entry, group, false, false);
                _logger.Log($"{folder}: Moved prefab to Addressable group {groupName}");
            }
        }

        #endregion
    }
}