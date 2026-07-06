using System.IO;
using System.Linq;
using Managers;
using Model.Data.Scene;
using Model.Enum.Named;
using SaveAndLoad;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Enum;

namespace Editor.CommandCenter.Screens.Modules.Validation
{
    public class SceneProfileValidationModule : IEditorValidationModule
    {
        public string ModuleName => "Scene Profile Validator";

        public ModuleStatus Status { get; private set; } = ModuleStatus.Unknown;

        private ICommandCenterLogger _logger;

        private const string SceneFolder = "Assets/Scenes";
        private const string ProfileFolder = "Assets/ScriptableObjects/SceneProfiles";
        private const string BootstrapSceneName = "Bootstrap";

        public void Initialize(ICommandCenterLogger logger)
        {
            _logger = logger;
        }

        public VisualElement CreateContent()
        {
            var root = new VisualElement();
            root.Add(new Label("Ensures SceneProfiles exist and are assigned to GameLoader."));
            return root;
        }

        #region Validate

        public void Validate()
        {
            var scenes = GetSceneNames()
                .Where(s => s != BootstrapSceneName)
                .ToArray();

            var profiles = GetAllProfiles();

            var allExist = true;
            var allCorrectPath = true;
            var allAssigned = true;

            foreach (var scene in scenes)
            {
                var profile = profiles.FirstOrDefault(p => p.name == scene);

                if (!profile)
                {
                    allExist = false;
                    _logger.LogWarning($"Missing SceneProfile for scene: {scene}");
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(profile);
                if (!path.StartsWith(ProfileFolder))
                {
                    allCorrectPath = false;
                    _logger.LogWarning($"SceneProfile '{scene}' is not in correct folder.");
                }

                if (IsAssignedToGameLoader(profile)) continue;
                allAssigned = false;
                _logger.LogWarning($"SceneProfile '{scene}' not assigned to GameLoader.");
            }

            Status = (allExist && allCorrectPath && allAssigned)
                ? ModuleStatus.Valid
                : ModuleStatus.Warning;

            if (Status == ModuleStatus.Valid)
                _logger.Log("SceneProfiles valid.");
        }

        #endregion

        #region Enforce

        public void Enforce()
        {
            EnsureProfileFolderExists();

            var scenes = GetSceneNames()
                .Where(s => s != BootstrapSceneName)
                .ToArray();

            foreach (var scene in scenes)
            {
                var profile = FindProfile(scene);

                if (!profile)
                {
                    profile = CreateProfile(scene);
                    _logger.Log($"Created SceneProfile: {scene}");
                }

                SyncProfile(profile, scene);
            }

            AssignAllToGameLoader();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Status = ModuleStatus.Valid;
            _logger.Log("SceneProfiles enforced.");
        }

        #endregion

        #region Helpers

        private static string[] GetSceneNames()
        {
            var guids = AssetDatabase.FindAssets("t:Scene", new[] { SceneFolder });

            return guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
        }

        private static SceneProfile[] GetAllProfiles()
        {
            var guids = AssetDatabase.FindAssets("t:SceneProfile");

            return guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SceneProfile>)
                .ToArray();
        }

        private static SceneProfile FindProfile(string sceneName)
        {
            return GetAllProfiles().FirstOrDefault(p => p.name == sceneName);
        }

        private static SceneProfile CreateProfile(string sceneName)
        {
            var profile = ScriptableObject.CreateInstance<SceneProfile>();
            profile.name = sceneName;

            var path = $"{ProfileFolder}/{sceneName}.asset";
            AssetDatabase.CreateAsset(profile, path);

            return profile;
        }

        private static void EnsureProfileFolderExists()
        {
            if (!AssetDatabase.IsValidFolder(ProfileFolder))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "SceneProfiles");
            }
        }

        private static bool IsAssignedToGameLoader(SceneProfile profile)
        {
            var loader = Object.FindAnyObjectByType<GameLoader>();

            if (!loader)
            {
                Debug.LogWarning("GameLoader not loaded in editor. Validation skipped.");
                return false;
            }

            var serialized = new SerializedObject(loader);
            var property = serialized.FindProperty("sceneProfiles");

            for (var i = 0; i < property.arraySize; i++)
            {
                if (property.GetArrayElementAtIndex(i).objectReferenceValue == profile)
                    return true;
            }

            return false;
        }

        private void AssignAllToGameLoader()
        {
            var loader = Object.FindAnyObjectByType<GameLoader>();
            if (!loader)
            {
                _logger.LogError("GameLoader not found in currently loaded scene.");
                return;
            }

            var gameManager = loader.GetComponent<GameManager>();
            if (!gameManager)
            {
                _logger.LogError("GameManager component not found on GameLoader.");
                return;
            }

            var profiles = GetAllProfiles()
                .Where(p => p.name != BootstrapSceneName)
                .ToArray();

            var serialized = new SerializedObject(loader);
            var property = serialized.FindProperty("sceneProfiles");

            serialized.Update();
            property.arraySize = profiles.Length;

            for (var i = 0; i < profiles.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = profiles[i];
            }

            serialized.ApplyModifiedProperties();

            gameManager.gameLoader = loader;

            EditorUtility.SetDirty(loader);
            EditorUtility.SetDirty(gameManager);
        }
        
        private void SyncProfile(SceneProfile profile, string sceneName)
        {
            var changed = false;

            if (System.Enum.TryParse(sceneName, out NamedScene parsedScene))
            {
                if (profile.sceneName != parsedScene)
                {
                    profile.sceneName = parsedScene;
                    changed = true;
                    _logger.Log($"Updated sceneName for {sceneName}");
                }
            }
            else
            {
                _logger.LogWarning($"No NamedScene enum value found for {sceneName}");
            }

            if (changed)
            {
                EditorUtility.SetDirty(profile);
            }
        }

        #endregion
    }
}