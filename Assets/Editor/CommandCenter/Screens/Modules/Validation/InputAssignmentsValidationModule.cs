using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameInput;
using Managers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Data.Settings;
using Utils.Data.Settings.Screen;
using Utils.Data.Settings.Utils.SO.Settings;
using Object = UnityEngine.Object;

namespace Editor.CommandCenter.Screens.Modules.Validation
{
    public class InputAssignmentsValidationModule : IEditorValidationModule
    {
        public string ModuleName => "Input Assignments Validator";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Unknown;

        private ICommandCenterLogger _logger;

        private const string SettingsFolder =
            "Assets/ScriptableObjects/Settings/InputSettings";

        private const string AssetPath =
            SettingsFolder + "/InputAssignments.asset";


        public void Initialize(ICommandCenterLogger logger)
        {
            _logger = logger;
        }


        public VisualElement CreateContent()
        {
            var root = new VisualElement();

            root.Add(
                new Label(
                    "Ensures InputAssignments object exists, initialized, and assigned to InputManager."));

            return root;
        }


        #region Validate

        public void Validate()
        {
            var inputManager =
                Object.FindAnyObjectByType<InputManager>();

            if (!inputManager)
            {
                Status = ModuleStatus.Error;
                _logger.LogError(
                    "No InputManager found in scene.");
                return;
            }


            var asset =
                AssetDatabase.LoadAssetAtPath<InputAssignments>(
                    AssetPath);


            if (!asset)
            {
                Status = ModuleStatus.Warning;
                _logger.LogWarning(
                    "InputAssignments asset not found.");
                return;
            }


            var requiredSettings =
                FindInputSettings();


            var valid = true;


            foreach (var setting in requiredSettings)
            {
                var exists =
                    asset.assignments.Any(
                        x => x.settingName == setting);


                if (!exists)
                {
                    valid = false;

                    _logger.LogWarning(
                        $"Missing input assignment: {setting}");
                }
            }


            if (inputManager.defaultInputAssignments != asset)
            {
                valid = false;

                _logger.LogWarning(
                    "InputAssignments not assigned to InputManager.");
            }


            Status =
                valid
                    ? ModuleStatus.Valid
                    : ModuleStatus.Warning;


            if (Status == ModuleStatus.Valid)
            {
                _logger.Log(
                    "InputAssignments valid.");
            }
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


            var asset =
                AssetDatabase.LoadAssetAtPath<InputAssignments>(
                    AssetPath);


            if (!asset)
            {
                asset =
                    ScriptableObject
                        .CreateInstance<InputAssignments>();

                AssetDatabase.CreateAsset(
                    asset,
                    AssetPath);

                _logger.Log(
                    "Created InputAssignments asset.");
            }


            InitializeAsset(asset);


            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();


            var inputManager =
                Object.FindAnyObjectByType<InputManager>();


            if (!inputManager)
            {
                _logger.LogError(
                    "No InputManager found in scene.");

                return;
            }


            var serialized =
                new SerializedObject(inputManager);


            var property =
                serialized.FindProperty(
                    "defaultInputAssignments");


            property.objectReferenceValue = asset;


            serialized.ApplyModifiedProperties();


            Status = ModuleStatus.Valid;


            _logger.Log(
                "InputAssignments enforced and assigned.");
        }


        private static void InitializeAsset(
            InputAssignments asset)
        {
            var requiredSettings =
                FindInputSettings();


            asset.assignments ??=
                new List<InputAssignmentEntry>();


            // Remove obsolete entries
            for (var i = asset.assignments.Count - 1; i >= 0; i--)
            {
                if (!requiredSettings.Contains(
                        asset.assignments[i].settingName))
                {
                    asset.assignments.RemoveAt(i);
                }
            }


            // Add missing entries
            foreach (var setting in requiredSettings)
            {
                if (asset.assignments.Any(
                        x => x.settingName == setting))
                {
                    continue;
                }


                asset.assignments.Add(
                    new InputAssignmentEntry
                    {
                        settingName = setting,
                        action = default,
                        baseValue = nameof(KeyCode.None),
                        altValue = nameof(KeyCode.None)
                    });
            }


            EditorUtility.SetDirty(asset);
        }


        #endregion


        private static string[] FindInputSettings()
        {
            const string root =
                "Assets/ScriptableObjects/Settings/Pages";


            var pages =
                AssetDatabase.FindAssets(
                        "t:SettingsPageSO",
                        new[] { root })
                    .Select(
                        AssetDatabase.GUIDToAssetPath)
                    .Select(
                        AssetDatabase.LoadAssetAtPath<SettingsPageSO>)
                    .Where(x => x != null);


            var keyBindingPage =
                pages.FirstOrDefault(
                    x => x.pageName.Equals(
                        "Key Bindings",
                        StringComparison.OrdinalIgnoreCase));


            if (keyBindingPage == null)
            {
                Debug.LogWarning(
                    "Could not find Key Bindings settings page.");

                return Array.Empty<string>();
            }


            return keyBindingPage.categories
                .SelectMany(x => x.items)
                .Where(x => x != null)
                .Select(x => x.settingName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToArray();
        }
    }
}