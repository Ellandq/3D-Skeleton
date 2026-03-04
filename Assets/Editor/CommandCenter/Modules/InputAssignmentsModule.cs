using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameInput;
using Managers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.SO.Settings;
using Object = UnityEngine.Object;

namespace Editor.CommandCenter.Modules
{
    public class InputAssignmentsModule : IEditorModule
    {
        public string ModuleName => "Input Assignments Validator";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Unknown;

        private ICommandCenterLogger _logger;

        private const string SettingsFolder = "Assets/ScriptableObjects/Settings/InputSettings";
        private const string AssetPath = SettingsFolder + "/InputAssignments.asset";

        public void Initialize(ICommandCenterLogger logger)
        {
            _logger = logger;
        }

        public VisualElement CreateContent()
        {
            var root = new VisualElement();
            root.Add(new Label("Ensures InputAssignments object exists, initialized, and assigned to InputManager."));
            return root;
        }

        #region Validate

        public void Validate()
        {
            var inputManager = Object.FindAnyObjectByType<InputManager>();
            if (!inputManager)
            {
                Status = ModuleStatus.Error;
                _logger.LogError("No InputManager found in scene.");
                return;
            }

            var asset = AssetDatabase.LoadAssetAtPath<InputAssignments>(AssetPath);
            if (!asset)
            {
                Status = ModuleStatus.Warning;
                _logger.LogWarning("InputAssignments asset not found.");
                return;
            }

            var keys = Enum.GetNames(typeof(PlayerAction)).ToList();
            var valid = true;

            foreach (var key in keys.Where(key =>
                         !asset.settingNames.Contains(key) || !asset.settingNames.Contains(key + "_Alt")))
            {
                valid = false;
                _logger.LogWarning($"Missing key or key_Alt: {key}");
            }

            if (inputManager.defaultInputAssignments != asset)
            {
                valid = false;
                _logger.LogWarning("InputAssignments not assigned to InputManager.");
            }

            Status = valid ? ModuleStatus.Valid : ModuleStatus.Warning;
            if (Status == ModuleStatus.Valid) _logger.Log("InputAssignments valid.");
        }

        #endregion

        #region Enforce

        public void Enforce()
        {
            if (!AssetDatabase.IsValidFolder(SettingsFolder))
            {
                Directory.CreateDirectory(SettingsFolder);
                AssetDatabase.Refresh();
            }

            var asset = AssetDatabase.LoadAssetAtPath<InputAssignments>(AssetPath);
            if (!asset)
            {
                asset = ScriptableObject.CreateInstance<InputAssignments>();
                AssetDatabase.CreateAsset(asset, AssetPath);
                _logger.Log("Created InputAssignments asset.");
            }

            InitializeAsset(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var inputManager = Object.FindObjectOfType<InputManager>();
            if (!inputManager)
            {
                _logger.LogError("No InputManager found in scene.");
                return;
            }

            var serialized = new SerializedObject(inputManager);
            var property = serialized.FindProperty("defaultInputAssignments");
            property.objectReferenceValue = asset;
            serialized.ApplyModifiedProperties();

            Status = ModuleStatus.Valid;
            _logger.Log("InputAssignments enforced and assigned.");
        }

        private static void InitializeAsset(InputAssignments asset)
        {
            var keys = Enum.GetNames(typeof(PlayerAction)).ToList();

            asset.settingNames = new List<string>();
            asset.defaultValues = new List<string>();

            foreach (var key in keys)
            {
                asset.settingNames.Add(key);
                asset.defaultValues.Add(KeyCode.None.ToString());

                asset.settingNames.Add(key + "_Alt");
                asset.defaultValues.Add(KeyCode.None.ToString());
            }

            EditorUtility.SetDirty(asset);
        }

        #endregion
    }
}