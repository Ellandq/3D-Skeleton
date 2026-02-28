using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Editor.CommandCenter.Utils;
using UnityEditor;
using UnityEngine.UIElements;

namespace Editor.CommandCenter.Modules
{
    public class SceneEnumModule : IEditorModule
    {
        public string ModuleName => "Scene Enum Validator";

        public ModuleStatus Status { get; private set; } = ModuleStatus.Unknown;

        private ICommandCenterLogger _logger;

        private const string SceneFolder = "Assets/Scenes";
        private const string EnumPath = "Assets/Scripts/Utils/Enum/NamedScene.cs";

        public void Initialize(ICommandCenterLogger logger)
        {
            _logger = logger;
        }

        public VisualElement CreateContent()
        {
            var root = new VisualElement();

            root.Add(new Label("Validates scene names and enforces enum sync."));

            return root;
        }

        public void Validate()
        {
            var sceneNames = GetScenes();
            var valid = sceneNames.All(IsValidEnumName);

            Status = valid ? ModuleStatus.Valid : ModuleStatus.Warning;

            if (valid)
                _logger.Log("Scene names valid.");
            else
                _logger.LogWarning("Invalid scene names detected.");
        }

        public void Enforce()
        {
            var sceneNames = GetScenes();
            var validNames = sceneNames.Select(MakeValidEnumName).ToArray();

            EnumSynchronizer.Synchronize(
                EnumPath,
                "Utils.Enum",
                "NamedScene",
                validNames,
                _logger);
            
            AssetDatabase.Refresh();

            Status = ModuleStatus.Valid;
            _logger.Log("NamedScene enum updated.");
        }

        private static string[] GetScenes()
        {
            var guids = AssetDatabase.FindAssets("t:Scene", new[] { SceneFolder });
            return guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
        }

        private static bool IsValidEnumName(string name)
        {
            return Regex.IsMatch(name, @"^[A-Za-z_][A-Za-z0-9_]*$");
        }

        private static string MakeValidEnumName(string name)
        {
            var valid = Regex.Replace(name, @"[^A-Za-z0-9_]", "_");

            if (char.IsDigit(valid[0]))
                valid = "_" + valid;

            return valid;
        }
    }
}