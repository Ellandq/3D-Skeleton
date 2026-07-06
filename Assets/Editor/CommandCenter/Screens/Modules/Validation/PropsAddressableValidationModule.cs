using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine.UIElements;

namespace Editor.CommandCenter.Screens.Modules.Validation
{
    public class PropsAddressableValidationModule : IEditorValidationModule
    {
        public string ModuleName => "Props Addressables";

        public ModuleStatus Status { get; private set; } = ModuleStatus.Unknown;

        private ICommandCenterLogger _logger;

        private const string PropsRoot = "Assets/Prefabs/Props/";

        public void Initialize(ICommandCenterLogger logger)
        {
            _logger = logger;
        }

        public VisualElement CreateContent()
        {
            return new Label("Validates prop prefabs and addressables.");
        }

        public void Validate()
        {
            Status = ModuleStatus.Valid;

            foreach (var prefabPath in GetPropPrefabs())
            {
                ValidatePrefab(prefabPath);
            }
        }

        public void Enforce()
        {
            Status = ModuleStatus.Valid;

            foreach (var prefabPath in GetPropPrefabs())
            {
                EnforcePrefab(prefabPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void ValidatePrefab(string prefabPath)
        {
            var info = GetPrefabInfo(prefabPath);

            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (!settings)
            {
                _logger.LogWarning("Addressables not configured.");
                Status = ModuleStatus.Warning;
                return;
            }
            
            var guid = AssetDatabase.AssetPathToGUID(prefabPath);
            var entry = settings.FindAssetEntry(guid);
            
            if (entry == null)
            {
                _logger.LogWarning(
                    $"Props: Missing Addressable {info.address}");

                Status = ModuleStatus.Warning;
                return;
            }
            
            if (entry.address != info.address)
            {
                _logger.LogWarning(
                    $"Props: Wrong address {entry.address}, expected {info.address}");

                Status = ModuleStatus.Warning;
            }

            if (entry.parentGroup.Name == info.group) return;
            _logger.LogWarning(
                $"Props: Wrong group {entry.parentGroup.Name}, expected {info.group}");

            Status = ModuleStatus.Warning;
        }

        private void EnforcePrefab(string prefabPath)
        {
            var info = GetPrefabInfo(prefabPath);

            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (!settings)
            {
                _logger.LogWarning("Addressables not configured.");
                Status = ModuleStatus.Warning;
                return;
            }

            var group = settings.groups
                .FirstOrDefault(g => g.Name == info.group);
            
            if (!group)
            {
                group = settings.CreateGroup(
                    info.group,
                    false,
                    false,
                    false,
                    null,
                    typeof(BundledAssetGroupSchema));

                _logger.Log(
                    $"Created Addressable group {info.group}");
            }
            
            var guid = AssetDatabase.AssetPathToGUID(prefabPath);
            var entry = settings.FindAssetEntry(guid);
            
            if (entry == null)
            {
                entry = settings.CreateOrMoveEntry(guid, group);

                entry.address = info.address;

                _logger.Log(
                    $"Props: Added {info.address}");
            }
            else
            {
                if (entry.address != info.address)
                {
                    entry.address = info.address;

                    _logger.Log(
                        $"Props: Fixed address {info.address}");
                }

                if (entry.parentGroup == group) return;
                settings.MoveEntry(
                    entry,
                    group,
                    false,
                    false);

                _logger.Log(
                    $"Props: Moved {info.address} to {info.group}");
            }
        }

        private static string[] GetPropPrefabs()
        {
            return AssetDatabase
                .FindAssets("t:Prefab", new[] { PropsRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
        }

        private static PropInfo GetPrefabInfo(string prefabPath)
        {
            var relative = prefabPath
                .Replace(PropsRoot, "")
                .Replace(".prefab", "");


            var directory = Path.GetDirectoryName(relative)
                ?.Replace("\\", "/") ?? "";


            var group = directory.Split('/')
                .FirstOrDefault();


            if (string.IsNullOrEmpty(group))
                group = "Default";


            return new PropInfo
            {
                address = relative,
                group = group
            };
        }

        private class PropInfo
        {
            public string address;
            public string group;
        }
    }
}